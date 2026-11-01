variable "project_id" {
  description = "The Google Cloud Project ID"
  type        = string
}

variable "region" {
  description = "The Google Cloud region for the resources"
  type        = string
  default     = "southamerica-east1"
}

variable "db_password" {
  description = "The database password used by the Cloud SQL user (the application receives it through the pipeline)"
  type        = string
  sensitive   = true
}

variable "delete_protection" {
  description = "Activate the delete protection or not"
  type        = bool
  default     = false
}