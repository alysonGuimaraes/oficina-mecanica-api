# Kubernetes - manifestos da API

Os manifestos ficam em `k8s/` e são organizados com **Kustomize**: uma base comum e um overlay por ambiente. Hoje existe um overlay, `production`, usado pelo workflow `release.yml`.

## Organização

```
k8s/
├── base/                         # o que é comum a qualquer ambiente
│   ├── kustomization.yaml
│   ├── deployment.yaml           # Deployment autogarage-api-deployment
│   ├── service.yaml              # Service autogarage-api-service
│   └── hpa.yaml                  # HorizontalPodAutoscaler autogarage-api-hpa
└── overlays/
    └── production/               # o que muda em produção
        ├── kustomization.yaml    # resources, configMapGenerator, secretGenerator, images
        ├── runtime-config.env    # gerado pela pipeline (ignorado pelo git)
        └── secret.env            # gerado pela pipeline (ignorado pelo git)
```

- **Base** define a forma dos recursos: imagem, portas, probes, limites e o balanceamento.
- **Overlay** define o que é específico de produção: a configuração, os segredos e a imagem a publicar.
- O `kustomization.yaml` do overlay referencia a base com `../../base`. Assim, uma mudança na base vale para todos os overlays.

## Recursos

### Deployment (`autogarage-api-deployment`)

| Item | Valor | Motivo |
|---|---|---|
| Imagem | `autogarage-api-image`, substituída pelo overlay pela imagem da tag | A pipeline fixa a imagem da versão que está sendo publicada |
| Porta | `containerPort: 8080` | A API escuta na porta 8080 (`ASPNETCORE_HTTP_PORTS=8080`) |
| `envFrom` | ConfigMap `autogarage-api-config` e Secret `autogarage-api-secrets` | Todas as configurações entram como variáveis de ambiente |
| Readiness | `GET /health` em 5 s, a cada 10 s | Só recebe tráfego quando a API responde |
| Liveness | `GET /health` em 15 s, a cada 20 s | Reinicia o pod se a API parar de responder |
| Usuário | `runAsUser: 1654`, sem root, sem escalonamento de privilégio | Reduz o impacto de uma eventual invasão |
| Sistema de arquivos | `readOnlyRootFilesystem: true`, com `/tmp` em `emptyDir` | A aplicação não grava na imagem; o `/tmp` é o único lugar com escrita |
| Capabilities | `drop: ALL` | Nenhuma capacidade extra do Linux |
| Recursos | requests `100m` CPU e `128Mi`; limits `500m` CPU e `512Mi` | Base de cálculo para o HPA e para o agendamento dos pods |

O Deployment **não** define `replicas`. Quem decide o número de réplicas é o HPA. Definir `replicas` no Deployment faria os dois disputarem o controle.

### Service (`autogarage-api-service`)

- Tipo `LoadBalancer`, porta 80 externa para a porta 8080 do pod.
- O seletor é `app: autogarage-api`, o mesmo rótulo do Deployment.
- Gera um IP público no GKE. É a porta de entrada da API, e hoje o tráfego é HTTP puro.

### HorizontalPodAutoscaler (`autogarage-api-hpa`)

- Escala o Deployment entre **1 e 2 réplicas**.
- Métricas: CPU e memória, ambas com média de utilização de **80%** sobre os requests do pod.
- Depende do metrics-server, que já vem instalado no GKE.
- Em conjunto com o autoscaler de nós do cluster (de 1 a 2 nós no node pool), o HPA cobre o pico de ordens de serviço sem intervenção manual.

### ConfigMap e Secret

A configuração é dividida em duas partes, pelo tipo de dado.

| Objeto | Gerado por | Conteúdo |
|---|---|---|
| ConfigMap `autogarage-api-config` | `configMapGenerator` a partir de `runtime-config.env` e de `literals` | Dados não sensíveis: `ASPNETCORE_ENVIRONMENT=Production`, `ASPNETCORE_HTTP_PORTS=8080`, `Settings__Jwt__ExpiresInMinutes`, `Settings__Brevo__Enabled`, `Settings__Brevo__FromEmail`, `Settings__Brevo__FromName` e o conteúdo do secret `RUNTIME_CONFIG` |
| Secret `autogarage-api-secrets` | `secretGenerator` a partir de `secret.env` | Dados sensíveis: `ConnectionStrings__DefaultConnection` (com a senha do banco), `Settings__Jwt__Key`, `Settings__Password__AdditionalKey`, `Settings__BootstrapAdmin__Email`, `Settings__BootstrapAdmin__Password` e `Settings__Brevo__ApiKey` |

Os nomes dos dois objetos recebem um sufixo com hash do conteúdo (por exemplo, `autogarage-api-config-fh29m27762`). Isso acontece porque o kustomize atualiza a referência no Deployment. Quando um valor muda, o nome muda e o Deployment faz um rollout sozinho. Sem o hash, o pod continuaria com a configuração antiga.

Os valores vêm dos secrets e variáveis do GitHub, e a pipeline os grava nos arquivos `runtime-config.env` e `secret.env` no momento do deploy. Esses arquivos não são versionados (ver `.gitignore`).

Observação: o Secret do Kubernetes é codificado em base64, não criptografado. No GKE, o etcd é criptografado em repouso pelo provedor.

## Como a pipeline aplica os manifestos

O job `deploy` do `release.yml` faz, nesta ordem:

1. Autentica no GCP e obtém as credenciais do cluster (`get-gke-credentials`).
2. Escreve `runtime-config.env` e `secret.env` no overlay. Falha se algum valor obrigatório estiver vazio ou se o `RUNTIME_CONFIG` tiver chaves reservadas.
3. Acrescenta ao `kustomization.yaml` do overlay um bloco `images:` com a imagem da tag que está sendo publicada.
4. Executa `kubectl apply -k k8s/overlays/production`. O `kubectl` traz o kustomize embutido, então nenhum binário extra é baixado.
5. Espera o rollout com `kubectl rollout status deployment/autogarage-api-deployment --timeout=180s`.
6. Em caso de falha, coleta pods, eventos e logs para diagnóstico.

O job roda no environment `production`, que exige aprovação manual.

## Como aplicar manualmente

Os arquivos `runtime-config.env` e `secret.env` precisam existir localmente, com os valores reais. Crie-os a partir dos exemplos abaixo, **sem commitar**:

```bash
cd k8s/overlays/production

cat > runtime-config.env <<'EOF'
ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT=Information
Settings__Jwt__ExpiresInMinutes=15
Settings__Brevo__Enabled=false
Settings__Brevo__FromEmail=<remetente-verificado>
Settings__Brevo__FromName=AutoGarage
EOF

cat > secret.env <<'EOF'
ConnectionStrings__DefaultConnection=Host=<ip-privado-do-banco>;Port=5432;Database=autogarage;Username=autogarage_admin;Password=<senha>
Settings__Jwt__Key=<chave-com-32-bytes-ou-mais>
Settings__Password__AdditionalKey=<pepper>
Settings__BootstrapAdmin__Email=<email-do-admin>
Settings__BootstrapAdmin__Password=<senha-do-admin>
Settings__Brevo__ApiKey=<api-key-do-brevo>
EOF

# imagem a publicar (substitua pela tag desejada)
cat >> kustomization.yaml <<'EOF'

images:
  - name: autogarage-api-image
    newName: <regiao>-docker.pkg.dev/<projeto>/autogarage-api-repo/autogarage-api
    newTag: v0.1.0
EOF

kubectl apply -k .
kubectl rollout status deployment/autogarage-api-deployment --timeout=180s
```

Antes de aplicar, confirme que o `kubectl` aponta para o cluster certo:

```bash
kubectl config current-context
```

Para verificar a configuração gerada sem aplicar, use `kubectl kustomize .`. A saída mostra os objetos com os valores em base64 no Secret.

Depois de aplicar, o `kustomization.yaml` do overlay já tem o bloco `images:` acrescentado. Como ele é um arquivo versionado, restaure-o com `git checkout -- kustomization.yaml` antes de commitar.

## Limitações conhecidas

- **Sem TLS.** O Service expõe HTTP puro na porta 80. Um Ingress com certificado gerenciado é o próximo passo.
- **Sem NetworkPolicy.** Qualquer pod do cluster alcança a API.
- **Redirect para HTTPS sem porta HTTPS.** A aplicação redireciona para HTTPS, mas o Service só expõe a porta 80. O redirect não tem para onde ir enquanto não houver TLS.
- **Chaves do DataProtection efêmeras.** A aplicação avisa que as chaves não são persistidas. Com uma réplica não há problema, mas com duas réplicas do HPA cada pod tem chaves diferentes. Isso afeta cookies e anti-forgery, não os tokens JWT.
- **Cloud SQL sem SSL obrigatório.** O Terraform não exige conexão com SSL. O relatório de configuração do Trivy aponta isso (GCP-0015).
- **Segredos em base64.** Adequado para o escopo do projeto; para um ambiente real, o próximo passo seria um gerenciador externo (Secret Manager com o driver CSI).
- **Um overlay.** Não há `staging` nem `development` no cluster. O ambiente de desenvolvimento roda com o docker compose.
