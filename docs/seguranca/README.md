# Segurança

Esta pasta guarda o relatório de vulnerabilidades do projeto.

- [Relatório de vulnerabilidades](relatorio-vulnerabilidades.md): gerado automaticamente a cada push na `main`.

## Como o relatório é gerado

O workflow `.github/workflows/security-report.yml` roda a cada push na `main` e analisa três fontes:

1. **Dependências NuGet:** `dotnet list package --vulnerable --include-transitive`.
2. **Imagem Docker:** Trivy sobre a imagem construída a partir do `Dockerfile`.
3. **Configuração:** Trivy sobre o `Dockerfile`, os manifestos do `k8s/` e o Terraform. Só entram os controles reprovados.

O gerador é `.github/scripts/gerar_relatorio_vulnerabilidades.py`. A versão do Trivy é fixada no workflow (`aquasec/trivy:0.76.1`).

O workflow não falha por causa dos achados. Eles precisam ser avaliados por um humano.

## Como ler o relatório

- A severidade é normalizada em Critical, High, Medium, Low e Unknown. O NuGet chama de "Moderate" o que o Trivy chama de MEDIUM, e os dois aparecem como Medium.
- "Corrigida em: sem correção" quer dizer que nenhuma versão corrigida está disponível.
- O escopo indica se a dependência é direta ou transitiva, ou se a vulnerabilidade está no sistema operacional da imagem ou em uma biblioteca.

## Não edite o relatório

O arquivo é sobrescrito a cada execução do workflow. O gerador lê e grava sempre nos mesmos caminhos, na raiz do repositório, e recebe apenas o SHA analisado. Para gerar uma cópia local, rode os três comandos na raiz do repositório. Depois, restaure o relatório oficial com `git checkout -- docs/seguranca/relatorio-vulnerabilidades.md`.

```bash
dotnet list src/AutoGarage.slnx package --vulnerable --include-transitive --format json > audit.json
# Trivy sobre a imagem e sobre a configuração: ver o workflow para os comandos exatos.
python .github/scripts/gerar_relatorio_vulnerabilidades.py --commit "$(git rev-parse HEAD)"
```
