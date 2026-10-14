# Pipeline CI/CD — Provisionamento e Deploy

Fluxo de execução para provisionar a infraestrutura completa (Terraform) e configurar o
cluster GKE com a aplicação, incluindo a instalação do External Secrets Operator (ESO).

Princípio geral: **Terraform é a plataforma** (cluster + identidade GCP); **a pipeline
orquestra o resto** (kubeconfig → imagem → ESO → app).

## Pré-requisitos (uma vez, fora da pipeline)

1. Billing habilitado no projeto.
2. Habilitar as APIs (o Terraform atual **não** faz isso):
   ```bash
   gcloud services enable \
     compute.googleapis.com container.googleapis.com sqladmin.googleapis.com \
     secretmanager.googleapis.com servicenetworking.googleapis.com \
     artifactregistry.googleapis.com cloudresourcemanager.googleapis.com iam.googleapis.com
   ```
3. Bucket de state criado + `terraform -chdir=infra init -migrate-state`.
4. Runner com: `gcloud`, `terraform`, `kubectl`, `helm`, `docker`, `kustomize`.
5. Autenticação do CI no GCP (ideal: **Workload Identity Federation**, sem chave).
6. Secrets no CI: `TF_VAR_db_password`, `TF_VAR_project_id` e a credencial de auth.

## Mapa de dependências

```
[1 Setup/Auth]
      |
      v
[2 Terraform lint/validate]
      |
      v
[3 Terraform plan -> apply]  --> cria cluster, SA GCP, Secret Manager, WI binding
      |
      v
[4 kubeconfig] (gcloud get-credentials)
      |
      v
[5 Build & push imagem]  --> Artifact Registry (tag = SHA exato)
      |
      v
[6 Instalar/atualizar ESO] (helm upgrade --install --wait)
      |
      v
[7 Gerar runtime-config.env + setar imagem no overlay]
      |
      v
[8 kubectl apply -k overlays/production]
      |
      v
[9 Smoke test / verify]
```

## Fases da pipeline (comandos)

### 1. Setup & Auth
```bash
gcloud auth activate-service-account --key-file="$GCP_SA_KEY_FILE"   # ou Workload Identity
gcloud config set project "$TF_VAR_project_id"
```

### 2. Validar Terraform
```bash
terraform -chdir=infra init -input=false
terraform -chdir=infra fmt -check
terraform -chdir=infra validate
```

### 3. Provisionar infra
```bash
terraform -chdir=infra plan  -input=false -out=tfplan
terraform -chdir=infra apply -input=false -auto-approve tfplan   # em prod, gate manual aqui
```
Captura dos outputs:
```bash
REGISTRY=$(terraform -chdir=infra output -raw artifact_registry_url)
CLUSTER=$(terraform  -chdir=infra output -raw kubernetes_cluster_name)
```

### 4. Configurar kubectl
```bash
gcloud container clusters get-credentials "$CLUSTER" \
  --region southamerica-east1 --project "$TF_VAR_project_id"
```

### 5. Build & push (imagem exata, nunca `latest`)
```bash
REGION=southamerica-east1
gcloud auth configure-docker "$REGION-docker.pkg.dev" --quiet
IMAGE="$REGISTRY/autogarage-api:$GIT_SHA"
docker build -t "$IMAGE" .
docker push "$IMAGE"
```

### 6. Instalar/atualizar ESO (cluster-wide, idempotente)
```bash
helm repo add external-secrets https://charts.external-secrets.io
helm repo update
helm upgrade --install external-secrets external-secrets/external-secrets \
  -n external-secrets --create-namespace --wait
```

### 7. Gerar artefatos de runtime
```bash
# runtime-config.env NAO versionado; vem de variaveis do CI (config nao-sensivel)
cat > k8s/overlays/production/runtime-config.env <<EOF
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__Default=Host=<db_private_ip>;Database=autogarage;Username=autogarage_admin;Password=<placeholder>
EOF

# fixa a imagem exata no overlay
kustomize edit set image autogarage-api-image="$IMAGE" \
  --kustomize-dir k8s/overlays/production
```
> O hash do ConfigMap gerado muda quando o `runtime-config.env` muda -> rollout
> automatico. **Nao** e necessario `kubectl rollout restart`.

### 8. Aplicar os manifestos da app
```bash
kubectl apply -k k8s/overlays/production
kubectl rollout status deployment/autogarage-api-deployment --timeout=120s
```

### 9. Verificação
```bash
kubectl get pods,svc
IP=$(kubectl get svc autogarage-api-service -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
curl -fsS "http://$IP/api/Service"
```

## Pontos de atenção

- **Ordem ESO -> app** e critica (CRDs). O `--wait` do Helm espera o controller; ainda
  assim o pod pode ficar alguns segundos em `CreateContainerConfigError` ate o Secret
  aparecer, e depois se recupera sozinho.
- **Imagem antes do apply** (senao `ImagePullBackOff`) e **APIs habilitadas antes do
  Terraform**.
- **Segredo nunca no git**: `db_password` via `TF_VAR_db_password`; `runtime-config.env`
  gerado no runner (`.gitignore` cobre `infra/*.tfvars`).
- **Idempotencia**: `terraform init/apply`, `helm upgrade --install` e `kubectl apply -k`
  sao re-executaveis -> serve tanto para bootstrap quanto para deploys subsequentes.
- **ConfigMaps antigos com hash** se acumulam a cada mudanca de config; opcional limpar
  com `--prune`.
