title: Release Workflow
descricao: Build, publicação, deploy e monitoramento no GKE com versionamento por tag

Arquivo: `.github/workflows/release.yml`

## Objetivo

Validar, construir, publicar e deployar a imagem **Docker** da API no **Google Kubernetes Engine (GKE)**. O workflow garante que apenas imagens com tag versão (`v*`) sejam deployadas e que o deploy seja **gated** por aprovação manual.

## Trigger

```yaml
on:
  push:
    tags:
      - "v*"
  workflow_dispatch:
    inputs:
      tag:
        description: "Tag to build and deploy (e.g. v0.1.0)"
        required: true
```

- **Push de tag:** Ideal para releases formais.
- **Workflow dispatch:** Permite re-deployar/reverter para uma tag específica (útil para rollback).

## Permissões

```yaml
permissions:
  contents: read
  id-token: write
```

## Concorrência

```yaml
concurrency:
  group: release-${{ github.event.inputs.tag || github.ref_name }}
  cancel-in-progress: false
```

Impede deploys simultâneos da mesma tag/versão. `cancel-in-progress: false` garante conclusão do deploy em andamento.

## Variáveis de Ambiente

```yaml
env:
  REGION: ${{ vars.GCP_REGION }}
  CLUSTER: autogarage-k8s-cluster
  REPO: autogarage-api-repo
  IMAGE_NAME: autogarage-api
  OVERLAY: k8s/overlays/production
  SOLUTION: src/AutoGarage.slnx
  IMAGE_TAG: ${{ github.event.inputs.tag || github.ref_name }}
  REF: ${{ github.event.inputs.tag || github.ref }}
```

| Variável | Descrição |
|---|---|
| `IMAGE_TAG` | Tag da imagem (ex: `v0.1.0`) |
| `REF` | Ref Git a ser utilizado no checkout (tag ou branch) |
| `OVERLAY` | Caminho do overlay Kustomize para produção |
| `REPO` | Repositório do Artifact Registry |

## Jobs

### Job 1: `check`

**Objetivo:** Verificar se a imagem já existe no **Artifact Registry**. Se já existir, pula `build` e `push` (imagens são tratadas como **imutáveis**).

Roda em `ubuntu-latest`.

#### Steps

| Step | Ação | Descrição |
|---|---|---|
| **Authenticate to Google Cloud** | `google-github-actions/auth@7c6bc770dae815cd3e87ee6cdf493a5fab2cc093` | Autentica via WIF. |
| **Setup gcloud** | `google-github-actions/setup-gcloud@aa5489c8933f4cc7a4f7d45035b3b1440c9c10db` | Instala/configura gcloud CLI. |
| **Resolve image URI** | Script | Monta URI: `$REGION-docker.pkg.dev/$GCP_PROJECT_ID/$REPO/$IMAGE_NAME:$IMAGE_TAG`. |
| **Check if the image already exists** | `gcloud artifacts docker images describe` | Se retornar sucesso, `image_exists=true`. Senão `false`. |
| **Summary** | Script | Adiciona resumo ao GitHub Step Summary. |

#### Output

```yaml
outputs:
  image_exists: ${{ steps.image_exists.outputs.image_exists }}
```

### Job 2: `build`

**Objetivo:** Validar e construir a imagem Docker caso não exista no Artifact Registry.

Roda em `ubuntu-latest`. **Apenas se** `needs.check.outputs.image_exists != 'true'`.

#### Steps

| Step | Ação | Descrição |
|---|---|---|
| **Checkout** | `actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1` | Clona no ref alvo (`REF`). |
| **Setup .NET** | `actions/setup-dotnet@a98b56852c35b8e3190ac28c8c2271da59106c68` | Instala .NET `10.0.x`. |
| **Restore** | `dotnet restore "$SOLUTION"` | Restaura dependências. |
| **Format check** | `dottag: dotnet format "$SOLUTION" --verify-no-changes --severity warn` | Verifica formatação. |
| **Build** | `dotnet build "$SOLUTION" --no-restore -c Release` | Compila. |
| **Test** | `dotnet test "$SOLUTION" --no-build - c Release` | Executa testes. |
| **Build container image** | `docker build -t "$IMAGE_URI" .` | Cria imagem Docker (a partir do `Dockerfile` na raiz). |
| **Save image** | `docker save "$IMAGE_URI" -o image.tar` | Salva imagem como artefato `.tar`. |
| **Upload image artifact** | `actions/upload-artifact@cf430e030ddbb5b0abf93d22962f4752f3646cd9` | Upload por 1 day de `image.tar`.

### Job 3: `push`

**Objetivo:** Carregar e publicar imagem ao **Artifact Registry**.

Roda em `ubuntu-latest`. **Necessário:** `check` e `build`. **Apenas se** `needs.check.outputs.image_exists != 'true'`.

#### Steps

| Step | Ação | Descrição |
|---|---|---|
| **Download image artifact** | `actions/download-artifact@d3f86a106a0bac45b974a628896c90dbdf5c8093` | Baixa `image.tar` do build. |
| **Authenticate to Google Cloud** | `google-github-actions/auth@7c6bc770dae815cd3e87ee6cdf493a2p093v` | Autentica. |
| **Setup gcloud** | `google-github-actions/setup-gcloud@aa5489c8933f4cc7a4f7d45035b3b1440c9c10db` | Configura CLI. |
| **Configure Docker for Artifact Registry** | `gcloud auth configure-docker "$REGION-docker.pkg.dev" --quiet` | Configura docker para push. |
| **Load and push image** | `docker load -i image.tar` & `docker push "$IMAGE_URI"` | Carrega e envia imagem. 

### Job 4: `deploy`

**Objetivo:** Deployar a imagem no GKE usando **Kustomize**. **Gated** pelo environment `production` (requer aprovação manual).

Roda em `ubuntu-latest`. **Dependente de:** `check`, `build`, `push`. 
**Condição:** `if: always() && (needs.check.outputs.image_exists == 'true' || needs.push.result == 'success')`

Ou seja, deploya mesmo se a image já existia (via `dispatch` ou reutilização).

#### Steps

| Step | Ação | Descrição |
|---|---|---|
| **Checkout** | `actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1` | Clona ref alvo. |
| **Authenticate to Google Cloud** | `google-github-actions/auth@7c6bc770dae815cd3e87ee6cdf493a2p093v` | Autentica. |
| **Get GKE credentials** | `google-github-actions/get-gke-credentials@3da1e46a907576cefaa90c484278bb5b259dd395` | Configura kubeconfig. |
| **Install kustomize** | Script curl | Instala Kustomize via script oficial. |
| **Write runtime config** | `printf '%s\n' "$RUNTIME_CONFIG" > "$OVERLAY/runtime-config.env" | Writes the secret in runtime-config.env in the overlay. Depende de `secrets.RUNTIME_CONFIG`. |
| **Pin image in the overlay** | `cd "$OVERLAY" & kustomize edit set image autogarage-api-image="$IMAGE_URI"` | Atualiza image tag no overlay. |
| **Deploy** | `kubectl apply -k "$OVERLAY"` & `kubectl rollout status deployment/autogarage-api-deployment --timeout=180s` | Aplica manifestos Kustomize e aguarda rollout completo. |
| **Debug rollout failure** | *se falha* | Coleta pods, deployment, events, logs, ExternalSecrets, SecretStore e logs do External Secrets Operator. Útil for debugging. 

## Fluxo Resumido

1. **Check** → Verifica imagem no Artifact Registry
2. **Build** (se não existe) → Valida, testa, builda imagem, salva artefato
3. **Push** (se não existe) → Carrega artefato e publica
4. **Deploy** → Atualiza overlay, aplica, aguarda rollout, **necessita aprovação manual** no environment `production`

## Pontos Importantes

- **Imagens imutáveis:** Nunca reescreve imagem existente. Usa tag como histórico.
- **Approval gate:** `environment: production` força aprovação manual antes de aplicar em GKE.
- **Kustomize:** Gerenciamento de manifests com overlays (`k8s/overlays/production`).
- **Runtime config:** Arquivo `runtime-config.env` criado via secret `RUNTIME_CONFIG` (provavelmente ligado ao **External Secrets**).
- **Rollback:** Com `workflow_dispatch` e tag existente, pode re-deployar tag antiga sem build/push.
- **Debug embutido:** Em caso de falha, coleta logs e eventos do cluster para facilitar investigação.
- **WIF & menores privilégios:** Uso de Workload Identity Federation e `permissions` restritas.
