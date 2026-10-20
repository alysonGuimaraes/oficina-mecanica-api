# ==========================================
# 1. VPC AND NETWORKING
# ==========================================
resource "google_compute_network" "main_vpc" {
  name                    = "autogarage-vpc"
  auto_create_subnetworks = false
}

# Public Subnet (For internet access components, Load Balancers, etc)
resource "google_compute_subnetwork" "public_subnet" {
  name          = "autogarage-public-subnet"
  ip_cidr_range = "10.0.1.0/24"
  region        = var.region
  network       = google_compute_network.main_vpc.id
}

# Private Subnet (For API Pods and Database communication only)
resource "google_compute_subnetwork" "private_subnet" {
  name                     = "autogarage-private-subnet"
  ip_cidr_range            = "10.0.2.0/24"
  region                   = var.region
  network                  = google_compute_network.main_vpc.id
  private_ip_google_access = true
}

# Cloud Router + Cloud NAT (outbound internet for private nodes)
resource "google_compute_router" "nat_router" {
  name    = "autogarage-nat-router"
  region  = var.region
  network = google_compute_network.main_vpc.id
}

resource "google_compute_router_nat" "nat" {
  name                               = "autogarage-nat"
  router                             = google_compute_router.nat_router.name
  region                             = var.region
  nat_ip_allocate_option             = "AUTO_ONLY"
  source_subnetwork_ip_ranges_to_nat = "ALL_SUBNETWORKS_ALL_IP_RANGES"

  depends_on = [
    google_compute_subnetwork.public_subnet,
    google_compute_subnetwork.private_subnet
  ]
}

# ==========================================
# 2. ARTIFACT REGISTRY
# ==========================================
resource "google_artifact_registry_repository" "api_repo" {
  location      = var.region
  repository_id = "autogarage-api-repo"
  description   = "Docker repository for AutoGarage API images"
  format        = "DOCKER"
}

# ==========================================
# 3. SECRET MANAGER
# ==========================================
resource "google_secret_manager_secret" "db_password_secret" {
  secret_id = "autogarage-db-password"
  replication {
    auto {}
  }
}

resource "google_secret_manager_secret_version" "db_password_version" {
  secret      = google_secret_manager_secret.db_password_secret.id
  secret_data = var.db_password
}

# ==========================================
# 4. CLOUD SQL (POSTGRESQL) - PRIVATE NETWORK
# ==========================================
# Required block to allow Cloud SQL to use private IP within our VPC
resource "google_compute_global_address" "private_ip_address" {
  name          = "autogarage-private-ip-address"
  purpose       = "VPC_PEERING"
  address_type  = "INTERNAL"
  prefix_length = 16
  network       = google_compute_network.main_vpc.id
}

resource "google_service_networking_connection" "private_vpc_connection" {
  network                 = google_compute_network.main_vpc.id
  service                 = "servicenetworking.googleapis.com"
  reserved_peering_ranges = [google_compute_global_address.private_ip_address.name]

  # If the API refuses to delete the connection because a producer (e.g. a
  # just-deleted Cloud SQL instance) still references it, remove the VPC peering
  # instead so the network and subnets can be destroyed without manual steps.
  deletion_policy = "REMOVE_PEERING"

  # Tear the peering down before the subnets/network.
  depends_on = [
    google_compute_subnetwork.public_subnet,
    google_compute_subnetwork.private_subnet
  ]
}

resource "google_sql_database_instance" "postgres_instance" {
  name                = "autogarage-db-instance"
  database_version    = "POSTGRES_15"
  region              = var.region
  deletion_protection = var.delete_protection

  settings {
    tier = "db-f1-micro"

    ip_configuration {
      ipv4_enabled    = false
      private_network = google_compute_network.main_vpc.id
    }
  }

  depends_on = [google_service_networking_connection.private_vpc_connection]
}

resource "google_sql_user" "db_user" {
  name     = "autogarage_admin"
  instance = google_sql_database_instance.postgres_instance.name
  password = var.db_password
}

# ==========================================
# 5. GKE CLUSTER (KUBERNETES)
# ==========================================
resource "google_container_cluster" "primary_cluster" {
  name                = "autogarage-k8s-cluster"
  location            = var.region
  deletion_protection = var.delete_protection

  # Delete default node pool to use a custom one
  remove_default_node_pool = true
  initial_node_count       = 1

  network    = google_compute_network.main_vpc.name
  subnetwork = google_compute_subnetwork.private_subnet.name

  # Ensure the cluster nodes only get private IPs
  private_cluster_config {
    enable_private_nodes    = true
    enable_private_endpoint = false
    master_ipv4_cidr_block  = "172.16.0.0/28"
  }

  workload_identity_config {
    workload_pool = "${var.project_id}.svc.id.goog"
  }
}

resource "google_container_node_pool" "primary_nodes" {
  name       = "autogarage-node-pool"
  location   = var.region
  cluster    = google_container_cluster.primary_cluster.name
  node_count = 1

  autoscaling {
    total_max_node_count = 2
  }

  node_config {
    machine_type = "e2-small"

    boot_disk {
      size_gb = 20
    }

    # Required scopes for nodes to pull images from Artifact Registry
    oauth_scopes = [
      "https://www.googleapis.com/auth/cloud-platform"
    ]
  }
}


# ==========================================
# 6. Service Account
# ==========================================
resource "google_service_account" "external_secrets_sa" {
  account_id   = "external-secrets-sa"
  display_name = "Service Account for External Secrets Operator"
}

resource "google_secret_manager_secret_iam_member" "secret_accessor" {
  secret_id = google_secret_manager_secret.db_password_secret.id
  role      = "roles/secretmanager.secretAccessor"
  member    = "serviceAccount:${google_service_account.external_secrets_sa.email}"

  depends_on = [
    google_container_cluster.primary_cluster
  ]
}

resource "google_service_account_iam_member" "workload_identity_binding" {
  service_account_id = google_service_account.external_secrets_sa.name
  role               = "roles/iam.workloadIdentityUser"
  member             = "serviceAccount:${var.project_id}.svc.id.goog[default/external-secrets-sa]"

  depends_on = [
    google_container_cluster.primary_cluster
  ]
}