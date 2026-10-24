# CI Workflow

Arquivo: `.github/workflows/ci.yml`

## Objetivo

Garantir qualidade e integridade do código em **todo push para `main`** e **todo Pull Request**. Executa verificação de formatação, build e testes sem deploy.

## Trigger

```yaml
on:
  push:
    branches: [main]
  pull_request:
```

## Permissões

```yaml
permissions:
  contents: read
```

Apenas leitura do repositório (princípio do menor privilégio).

## Variáveis

```yaml
env:
  SOLUTION: src/AutoGarage.slnx
```

Aponta para a solução `.slnx` do .NET 10.

## Job: `analyze-build-test`

Roda em `ubuntu-latest`.

### Steps

| Step | Ação | Descrição |
|---|---|---|
| **Checkout** | `actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1` | Clona o repositório. |
| **Setup .NET** | `actions/setup-dotnet@a98b56852c35b8e3190ac28c8c2271da59106c68` | Instala .NET SDK `10.0.x`. |
| **Restore** | `dotnet restore "$SOLUTION"` | Restaura dependências NuGet. |
| **Format check** | `dotnet format "$SOLUTION" --verify-no-changes --severity warn` | Verifica formatação. **Falha se houver arquivos não formatados**. |
| **Build** | `dotnet build "$SOLUTION" --no-restore -c Release` | Compila em modo Release sem restaurar novamente. |
| **Test** | `dotnet test "$SOLUTION" --no-build -c Release --logger "trx;LogFileName=results.trx"` | Executa testes unitários usando os assemblies já buildados. Gera `.trx`. |
| **Upload test results** | `actions/upload-artifact@cf430e030ddbb5b0abf93d22962f4752f3646cd9` | Sobe os relatórios `.trx` como artefato (sempre, mesmo em falha). |

## Quando usar

- **Obrigatório em PRs:** Garante que formatação, build e testes estão corretos antes do merge.
- **Proteção da `main`:** Ideal para configurar branch protection exigindo este workflow como check.

## Pontos Importantes

- **`--no-restore` / `--no-build`**: Otimiza tempo evitando etapas redundantes.
- **`--verify-no-changes`**: Impõe padronização de código via `dotnet format`.
- **Artefatos sempre enviados**: Facilita análise de falhas de teste.
- **Sem acesso a cloud/secrets**: Completamente isolado e seguro para PRs vindos de forks (apenas `contents: read`). 
