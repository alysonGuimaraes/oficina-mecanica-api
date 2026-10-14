terraform {
  backend "gcs" {
    bucket = "autogarage-tfstate-production"
    prefix = "terraform/state"
  }
}
