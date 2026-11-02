# Security Report Workflow

Arquivo: `.github/workflows/security-report.yml`

## Objetivo

Gerar, a cada push na `main`, um relatório de vulnerabilidades do projeto e gravá-lo no repositório em `docs/seguranca/relatorio-vulnerabilidades.md`. O relatório reúne três fontes:

1. **Dependências NuGet**, pela auditoria do próprio .NET.
2. **Imagem Docker**, pelo Trivy, sobre a imagem construída a partir do `Dockerfile`.
3. **Configuração**, pelo Trivy, sobre `Dockerfile`, manifestos Kubernetes e Terraform.

O workflow não é um gate: ele nunca reprova um commit por causa de achados. Os achados são para avaliação humana.

## Gatilhos

| Gatilho | Condição |
|---|---|
| `push` | Na branch `main`, exceto quando a alteração toca apenas `docs/seguranca/**`. |
| `workflow_dispatch` | Execução manual, pela aba **Actions**. |

O filtro `paths-ignore` impede que o commit do próprio bot dispare outra execução. O commit também leva `[skip ci]`, para que o `ci.yml` não rode por causa dele.

## Permissões e concorrência

- `permissions: contents: write`: necessário para o commit e o push do relatório.
- `concurrency` com o grupo `security-report` e `cancel-in-progress: false`: duas execuções nunca se sobrepõem, e uma execução em andamento não é cancelada por outra.

## Variáveis de ambiente

| Variável | Valor | Função |
|---|---|---|
| `SOLUTION` | `src/AutoGarage.slnx` | Solução analisada pela auditoria NuGet. |
| `REPORT` | `docs/seguranca/relatorio-vulnerabilidades.md` | Arquivo gerado e commitado. |
| `TRIVY_IMAGE` | `aquasec/trivy:0.76.1` | Versão do Trivy, fixada pela tag da imagem. |
| `APP_IMAGE` | `autogarage-api:scan` | Nome local da imagem construída no runner. |

## Passos

| Passo | O que faz | Saída |
|---|---|---|
| Checkout | Baixa o commit que disparou a execução. | — |
| Setup .NET | Instala o SDK `10.0.x`. | — |
| Restore | `dotnet restore` da solução. | — |
| Audit NuGet packages | `dotnet list ... package --vulnerable --include-transitive --format json`. | `audit.json` |
| Build application image | `docker build` com o `Dockerfile` do repositório. | Imagem `autogarage-api:scan` |
| Save application image | `docker save` da imagem para um arquivo. | `image.tar` |
| Scan image with Trivy | `trivy image --input` sobre o arquivo salvo. | `trivy-image.json` |
| Scan configuration with Trivy | `trivy config` sobre o repositório, ignorando `tests/`. | `trivy-config.json` |
| Write report | Executa o gerador Python com os três arquivos. | `docs/seguranca/relatorio-vulnerabilidades.md` |
| Commit report to main | Faz `git add` do relatório. Se não houver diferença, termina sem commit. Senão, commita e envia para `main`. | Commit do bot |

### Por que a imagem é salva em arquivo

O Trivy roda dentro de um container. Para ele enxergar a imagem construída no runner, a imagem é salva em `image.tar` e passada ao Trivy com `--input`. Assim não é preciso montar o socket do Docker no container do Trivy.

## Gerador do relatório

Arquivo: `.github/scripts/gerar_relatorio_vulnerabilidades.py`

- **Entradas fixas:** `audit.json`, `trivy-image.json` e `trivy-config.json`, na raiz do repositório. O único argumento aceito é `--commit`, com o SHA analisado.
- **Saída fixa:** `docs/seguranca/relatorio-vulnerabilidades.md`, sobrescrita a cada execução.
- **Normalização de severidade:** `CRITICAL` vira `Critical`, `HIGH` vira `High`, `MEDIUM` e `MODERATE` viram `Medium`, `LOW` vira `Low`, e qualquer outro valor vira `Unknown`.
- **Ordenação:** por severidade, depois por alvo e pacote.

### Conteúdo do relatório

- Cabeçalho com a data de geração (UTC) e o commit analisado.
- Uma seção por fonte, cada uma com:
  - uma tabela-resumo por severidade;
  - uma tabela detalhada com severidade, alvo, pacote ou tipo, versão, versão corrigida, escopo ou descrição e identificador.
- Se uma fonte não tiver achados, a seção diz **Nenhum achado**.

### Regras específicas por fonte

- **NuGet:** a versão corrigida não é informada, porque a auditoria do `dotnet list` não a fornece.
- **Imagem:** o alvo mostra só a distribuição, por exemplo `imagem (ubuntu 24.04)`, sem o caminho do arquivo temporário. Vulnerabilidades sem correção aparecem como `sem correção`.
- **Configuração:** só entram controles reprovados (`Status: FAIL`). Os aprovados não são achados.

## Commit e comportamento do repositório

- O relatório inclui a data e o SHA analisado. Por isso, quase toda execução muda o arquivo e gera um commit na `main`.
- Se o relatório não mudar, o passo de commit termina sem criar nada.
- O commit usa o usuário `github-actions[bot]`.

### Requisito de proteção de branch

O push vai direto para `main`. Se a branch estiver protegida, o bot precisa estar autorizado a enviar para ela. Sem isso, o passo de commit falha.

## Quando o workflow falha

- **Não falha por achados.** Vulnerabilidades encontradas não interrompem a execução.
- **Falha se alguma ferramenta quebrar:** restore, auditoria, build da imagem, Trivy, gerador ou push. Nesse caso o relatório não é atualizado.

## Como reproduzir localmente

O procedimento está em [docs/seguranca/README.md](../seguranca/README.md), na seção "Não edite o relatório". Ele roda o mesmo gerador, a partir da raiz do repositório, depois de gerar os três arquivos JSON.

## Limitações conhecidas

- **Sem gate de severidade.** Não há limite que bloqueie o pipeline.
- **Trivy baixa a base de vulnerabilidades a cada execução.** Isso aumenta o tempo do workflow.
- **Build da imagem a cada push na `main`.** O tempo do workflow inclui o build completo do `Dockerfile`.
- **Trivy fixado pela tag, não pelo SHA.** Uma nova publicação com a mesma tag mudaria o scanner sem aviso.
- **Sem análise de segredos.** O workflow não procura credenciais no código.
- **Achados repetidos entre fontes.** O Trivy sobre a imagem também lê os `deps.json` do .NET, então uma mesma dependência pode aparecer na auditoria NuGet e na seção de imagem. As seções são separadas de propósito, para que cada fonte fique clara.

## Ver também

- [Relatório atual](../seguranca/relatorio-vulnerabilidades.md)
- [Pasta de segurança](../seguranca/README.md)
- [CI Workflow](ci-workflow.md)
- [Release Workflow](release-workflow.md)
