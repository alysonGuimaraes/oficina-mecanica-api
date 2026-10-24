# Infrastructure Workflow

Arquivo: `.github/workflows/infra.yml`

## Objetivo

Provisionar, configurar ou destruir toda a stack GCP necessária para rodar o **AutoGarage**. É 100% **manual** (`workflow_dispatch`), voltado para estudo e bootstrap da infraestrutura.

## Trigger

```yaml
on:
  workflow_dispatch:
    inputs:
      action:
        description: "Terraform action to run"
        type: choice
        options:
          - apply
          - destroy
        default: apply
        required: true
```

## Permissões

```yaml
permissions:
  contents: read
  id-token: write
```

- `contents: read`: Checkout do repositório
- `id-token: write`: Necessário para autenticação via **Workload Identity Federation**

## Concorrência

```yaml
concurrency:
  group: infrastructure
  cancel-in-progress: false
```

Garante **apenas uma execução por vez**. Nunca cancela operações em andamento (evita corrupções no estado Terraform).

## Variáveis de Ambiente

```yaml
env:
  TF_IN_AUTOMATION: "true"
  TF_VAR_project_id: ${{ secrets.GCP_PROJECT_ID }}
  TF_VAR_db_password: ${{ secrets.DB_PASSWORD }}
  TF_VAR_region: ${{ vars.GCP_REGION }}
```

Passa variáveis diretamente para o Terraform via `TF_VAR_*`.

## Job 1: `terraform`

Roda em `ubuntu-latest`, diretório `infra/`.

### Steps

| Step | Ação | Descrição |
|---|---|---|
| **Checkout** | `actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1` | Clona o repositório. |
| **Authenticate to Google Cloud** | `google-github-actions/auth@7c6bc770dae815cd3e89ee6cdf493a5fab2cc093` | Autentica via WIF com `GCP_WORKLOAD_IDENTITY_PROVIDER` e `GCP_SERVICE_ACCOUNT`. |
| **Setup Terraform** | `hashicorp/setup-terraform@dfe3c3f87815947d99a8997f908cb6525fc44e9e` | Instala Terraform `1.16.4`. |
| **Terraform Init** | `terraform init -input=false` | Inicializa backend e providers. |
| **Terraform Validate** | `terraform validate` | Valida configuração Terraform. |
| **Terraform Apply** | *apenas se `action == 'apply'`* | `terraform plan -out=tfplan` seguido de `terraform apply -auto-approve tfplan`. |
| **Terraform Destroy** | *apenas se `action == 'destroy'`* | `terraform destroy -auto-approve`. |

## Job 2: `configure-cluster`

Depende de `terraform`. **Executado apenas quando `apply`**.

Roda em `ubuntu-latest` com env:

```yaml
env:
  REGION: ${{ vars.GCP_REGION }}
  CLUSTER: autogarage-k8s-cluster
```

### Steps

| Step | Ação | Descrição |
|---|---|---|
| **Checkout** | `actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1` | Clona repositório. |
| **Authenticate to Google Cloud** | `google-github-actions/auth@7c6bc770dae815cd3e89ee6cdf493a5fab2cc093` | Autentica no GCP. |
| **Get GKE credentials** | `google-github-actions/get-gke-credentials@3da1e46a907576cefaa90c484278bb5b259dd395` | Configura `kubeconfig` para acessar o cluster. |
| **Wait for nodes to be ready** | `kubectl wait --for=condition=Ready nodes --all --timeout=5m` | Aguarda nós do GKE ficarem prontos. |
| **Setup Helm** | `azure/setup-helm@9bc31f4ebc9c6b171d7bfbaa5d006ae7abdb4310` | Instala Helm. |
| **Install External Secrets Operator** | *Helm upgrade/install* | Adiciona repo `external-secrets`, atualiza e instala via Helm no namespace `external-seops`/`external-secrets`. (conforme step: `external-secrets --namespace external-secrets --create-namespace`) |

## Quando usar

- **Bootstrap inicial:** `apply` para criar toda infraestrutura (VPC, GKE, Artifact Registry, Cloud SQL, etc.)
- **Zerar ambiente:** `destroy` para remover tudo (cuidado com dados!)
- **Atualizar add-ons:** Caso necessário após recriação do cluster, pode rodar manualmente.

## Pontos Importantes

- **Manual:** Não roda automaticamente. Evita custos acidentais.
- **Estado Terraform:** Deve utilizar backend remoto (geralmente GCS) para controle compartilhado.
- **Idempotente:** `terraform plan/apply` garantem configuração desejada.
- **Add-ons críticos:** **External Secrets Operator** é instalado para integrar com **Secret Manager/External Secrets** no deploy.
- **Aprovação segura:** `cancel-in-progress: false` garante operação completa. 
