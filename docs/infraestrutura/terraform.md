# Terraform - infraestrutura na GCP

Os arquivos ficam em `infra/` e provisionam a infraestrutura de rede, o banco de dados, o cluster Kubernetes (GKE) e o registro de imagens (Artifact Registry). O estado é guardado remotamente, em um bucket do Cloud Storage.

A pipeline `infra.yml` roda o Terraform de forma manual (`workflow_dispatch`), com as ações `apply` e `destroy`. Este documento explica também como rodar o mesmo processo à mão.

## Arquivos

| Arquivo | Conteúdo |
|---|---|
| `providers.tf` | Provider `hashicorp/google` na versão `8.6.0` e o projeto e a região padrão |
| `backend.tf` | Estado remoto no bucket `autogarage-tfstate-production`, prefixo `terraform/state` |
| `variables.tf` | Variáveis de entrada (projeto, região, senha do banco e proteção contra exclusão) |
| `main.tf` | Recursos da infraestrutura |
| `outputs.tf` | Valores exibidos ao final do `apply` |
| `terraform.tfvars` | Valores locais (ignorado pelo git; não commitar) |

## Pré-requisitos

- Terraform `1.16.4` (a mesma versão da pipeline).
- `gcloud` autenticado na conta que tem permissão no projeto: `gcloud auth login` e `gcloud auth application-default login`.
- O bucket de estado `autogarage-tfstate-production` já criado. O Terraform não cria o próprio bucket de estado.
- APIs habilitadas no projeto: Compute Engine, Service Networking, Cloud SQL Admin, Kubernetes Engine e Artifact Registry. O Terraform **não** habilita essas APIs. Em um projeto novo, habilite antes com `gcloud services enable`.

## Variáveis

| Variável | Obrigatória | Padrão | Uso |
|---|---|---|---|
| `project_id` | sim | — | ID do projeto GCP |
| `region` | não | `southamerica-east1` | Região de todos os recursos |
| `db_password` | sim | — | Senha do usuário `autogarage_admin` do Cloud SQL. É marcada como `sensitive` |
| `delete_protection` | não | `false` | Protege o cluster e o banco contra exclusão quando `true` |

Na pipeline, as variáveis vêm de `TF_VAR_project_id` (secret `GCP_PROJECT_ID`), `TF_VAR_db_password` (secret `DB_PASSWORD`) e `TF_VAR_region` (variável `GCP_REGION`).

## Como aplicar manualmente

```bash
cd infra

export TF_VAR_project_id="<id-do-projeto>"
export TF_VAR_db_password="<senha-do-banco>"
export TF_VAR_region="southamerica-east1"   # use a mesma região da variável GCP_REGION

terraform init
terraform validate
terraform plan -out=tfplan
terraform apply tfplan
```

Depois do `apply`, os outputs mostram o registro de imagens, o IP privado do banco e o nome do cluster.

Para conectar o `kubectl` ao cluster:

```bash
gcloud container clusters get-credentials autogarage-k8s-cluster \
  --region "$TF_VAR_region" --project "$TF_VAR_project_id"
```

Para remover tudo, primeiro confirme que `delete_protection` é `false` e depois rode `terraform destroy`. Os dados do banco somem junto.

## Recursos criados e o motivo de cada um

### Rede

| Recurso | Motivo |
|---|---|
| `google_compute_network.main_vpc` (`autogarage-vpc`) | Rede isolada para o cluster e o banco. Sem sub-redes automáticas, para controlar os intervalos de IP. |
| `google_compute_subnetwork.public_subnet` (`10.0.1.0/24`) | Reservada para componentes voltados à internet. Hoje nenhum recurso a usa diretamente. |
| `google_compute_subnetwork.private_subnet` (`10.0.2.0/24`) | Sub-rede do cluster. Com `private_ip_google_access`, os nós acessam APIs do Google sem IP público. |
| `google_compute_router.nat_router` e `google_compute_router_nat.nat` | Saída para a internet dos nós privados, que precisam baixar imagens, falar com APIs externas (ex.: envio de e-mails pela API do Brevo) e sem expor IP público. O NAT usa `AUTO_ONLY`, então o IP de saída pode mudar. Se a chave de API do Brevo for restrita a IPs autorizados, reserve um IP estático para o NAT antes de aplicar essa restrição. |

### Registro de imagens

| Recurso | Motivo |
|---|---|
| `google_artifact_registry_repository.api_repo` (`autogarage-api-repo`, formato Docker) | Armazena as imagens publicadas pela pipeline de release, com tag igual à versão (`v*`). |

### Banco de dados

| Recurso | Motivo |
|---|---|
| `google_compute_global_address.private_ip_address` (`/16`, `VPC_PEERING`) | Bloco de IP reservado para os serviços gerenciados do Google na VPC. |
| `google_service_networking_connection.private_vpc_connection` | Peering entre a VPC e a rede do Cloud SQL. É o que dá ao banco um IP privado. |
| `google_sql_database_instance.postgres_instance` (`autogarage-db-instance`, PostgreSQL 15, `db-f1-micro`, sem IP público) | Banco da aplicação, acessível só pela VPC. `POSTGRES_15` acompanha a imagem `postgres:15` do ambiente de testes. |
| `google_sql_user.db_user` (`autogarage_admin`) | Usuário da aplicação, com a senha vinda de `db_password`. |

O banco de dados `autogarage` não é criado pelo Terraform. A própria API cria o banco no startup, o que exige que o usuário tenha permissão para criar bancos.

### Cluster Kubernetes

| Recurso | Motivo |
|---|---|
| `google_container_cluster.primary_cluster` (`autogarage-k8s-cluster`) | Cluster GKE onde a API roda. O node pool padrão é removido, e o pool próprio é criado em seguida. |
| Nós privados (`enable_private_nodes`), endpoint público, CIDR do control plane `172.16.0.0/28` | Os nós não têm IP público, mas o `kubectl` da pipeline acessa o control plane. |
| `workload_identity_config` (`<projeto>.svc.id.goog`) | Habilita Workload Identity no cluster. Hoje nenhum workload o usa, porque o ESO foi removido. |
| `google_container_node_pool.primary_nodes` (`autogarage-node-pool`, `e2-medium`, 1 nó, autoescala até 2) | Capacidade de execução dos pods. A autoescala de nós complementa o HPA, que escala pods. |

## Valores de saída

| Output | Uso |
|---|---|
| `artifact_registry_url` | Prefixo das imagens (`<região>-docker.pkg.dev/<projeto>/autogarage-api-repo`) |
| `database_private_ip` | IP privado do Cloud SQL. Não é usado diretamente pela pipeline: ela consulta o mesmo valor com `gcloud sql instances describe` |
| `kubernetes_cluster_name` | Nome do cluster para o `get-credentials` |
