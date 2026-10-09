output "artifact_registry_url" {
  description = "The URL of the Artifact Registry repository"
  value       = "${var.region}-docker.pkg.dev/${var.project_id}/${google_artifact_registry_repository.api_repo.repository_id}"
}

output "database_private_ip" {
  description = "The internal IP address of the Cloud SQL instance"
  value       = google_sql_database_instance.postgres_instance.private_ip_address
}

output "kubernetes_cluster_name" {
  description = "The name of the GKE cluster"
  value       = google_container_cluster.primary_cluster.name
}