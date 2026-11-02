# Relatório de vulnerabilidades

- Gerado em: 2026-11-02 12:47 UTC
- Commit analisado: `8746265beba4c8d1f6fa779bcde73d3fc0113aa2`

Arquivo gerado automaticamente pelo workflow `security-report.yml` a cada push na `main`. Não edite manualmente.

## Dependências NuGet

Fonte: `dotnet list package --vulnerable --include-transitive`, com a base de advisories do NuGet.

| Severidade | Quantidade |
|---|---|
| High | 1 |
| Medium | 1 |

| Severidade | Alvo | Pacote ou tipo | Versão | Corrigida em | Escopo ou descrição | Identificador |
|---|---|---|---|---|---|---|
| High | AutoGarage.Api.csproj | System.Security.Cryptography.Pkcs | 9.0.0 | - | transitiva | [GHSA-846f-f764-76mx](https://github.com/advisories/GHSA-846f-f764-76mx) |
| Medium | AutoGarage.Api.csproj | System.Security.Cryptography.Pkcs | 9.0.0 | - | transitiva | [GHSA-2fmv-rv2c-59x7](https://github.com/advisories/GHSA-2fmv-rv2c-59x7) |

## Imagem Docker

Fonte: Trivy, sobre a imagem construída a partir do `Dockerfile` (sistema operacional e bibliotecas do runtime).

| Severidade | Quantidade |
|---|---|
| Medium | 21 |
| Low | 5 |

| Severidade | Alvo | Pacote ou tipo | Versão | Corrigida em | Escopo ou descrição | Identificador |
|---|---|---|---|---|---|---|
| Medium | imagem (ubuntu 24.04) | dash | 0.5.12-6ubuntu5 | sem correção | sistema operacional | [CVE-2026-102473](https://avd.aquasec.com/nvd/cve-2026-102473) |
| Medium | imagem (ubuntu 24.04) | dash | 0.5.12-6ubuntu5 | sem correção | sistema operacional | [CVE-2026-102474](https://avd.aquasec.com/nvd/cve-2026-102474) |
| Medium | imagem (ubuntu 24.04) | libc-bin | 2.39-0ubuntu8.9 | sem correção | sistema operacional | [CVE-2026-18374](https://avd.aquasec.com/nvd/cve-2026-18374) |
| Medium | imagem (ubuntu 24.04) | libc-bin | 2.39-0ubuntu8.9 | sem correção | sistema operacional | [CVE-2026-89092](https://avd.aquasec.com/nvd/cve-2026-89092) |
| Medium | imagem (ubuntu 24.04) | libc6 | 2.39-0ubuntu8.9 | sem correção | sistema operacional | [CVE-2026-18374](https://avd.aquasec.com/nvd/cve-2026-18374) |
| Medium | imagem (ubuntu 24.04) | libc6 | 2.39-0ubuntu8.9 | sem correção | sistema operacional | [CVE-2026-89092](https://avd.aquasec.com/nvd/cve-2026-89092) |
| Medium | imagem (ubuntu 24.04) | libgnutls30t64 | 3.8.3-1.1ubuntu3.6 | sem correção | sistema operacional | [CVE-2026-67693](https://avd.aquasec.com/nvd/cve-2026-67693) |
| Medium | imagem (ubuntu 24.04) | libgnutls30t64 | 3.8.3-1.1ubuntu3.6 | sem correção | sistema operacional | [CVE-2026-88647](https://avd.aquasec.com/nvd/cve-2026-88647) |
| Medium | imagem (ubuntu 24.04) | libgnutls30t64 | 3.8.3-1.1ubuntu3.6 | sem correção | sistema operacional | [CVE-2026-88648](https://avd.aquasec.com/nvd/cve-2026-88648) |
| Medium | imagem (ubuntu 24.04) | libgnutls30t64 | 3.8.3-1.1ubuntu3.6 | sem correção | sistema operacional | [CVE-2026-95184](https://avd.aquasec.com/nvd/cve-2026-95184) |
| Medium | imagem (ubuntu 24.04) | libgnutls30t64 | 3.8.3-1.1ubuntu3.6 | sem correção | sistema operacional | [CVE-2026-95209](https://avd.aquasec.com/nvd/cve-2026-95209) |
| Medium | imagem (ubuntu 24.04) | libgnutls30t64 | 3.8.3-1.1ubuntu3.6 | sem correção | sistema operacional | [CVE-2026-95210](https://avd.aquasec.com/nvd/cve-2026-95210) |
| Medium | imagem (ubuntu 24.04) | libpcre2-8-0 | 10.42-4ubuntu2.1 | sem correção | sistema operacional | [CVE-2026-103111](https://avd.aquasec.com/nvd/cve-2026-103111) |
| Medium | imagem (ubuntu 24.04) | libpcre2-8-0 | 10.42-4ubuntu2.1 | sem correção | sistema operacional | [CVE-2026-86145](https://avd.aquasec.com/nvd/cve-2026-86145) |
| Medium | imagem (ubuntu 24.04) | libpcre2-8-0 | 10.42-4ubuntu2.1 | sem correção | sistema operacional | [CVE-2026-89156](https://avd.aquasec.com/nvd/cve-2026-89156) |
| Medium | imagem (ubuntu 24.04) | libpcre2-8-0 | 10.42-4ubuntu2.1 | sem correção | sistema operacional | [CVE-2026-89157](https://avd.aquasec.com/nvd/cve-2026-89157) |
| Medium | imagem (ubuntu 24.04) | libpcre2-8-0 | 10.42-4ubuntu2.1 | sem correção | sistema operacional | [CVE-2026-89158](https://avd.aquasec.com/nvd/cve-2026-89158) |
| Medium | imagem (ubuntu 24.04) | libpcre2-8-0 | 10.42-4ubuntu2.1 | sem correção | sistema operacional | [CVE-2026-89160](https://avd.aquasec.com/nvd/cve-2026-89160) |
| Medium | imagem (ubuntu 24.04) | libpcre2-8-0 | 10.42-4ubuntu2.1 | sem correção | sistema operacional | [CVE-2026-89161](https://avd.aquasec.com/nvd/cve-2026-89161) |
| Medium | imagem (ubuntu 24.04) | tar | 1.35+dfsg-3ubuntu0.4 | sem correção | sistema operacional | [CVE-2026-18477](https://avd.aquasec.com/nvd/cve-2026-18477) |
| Medium | imagem (ubuntu 24.04) | tar | 1.35+dfsg-3ubuntu0.4 | sem correção | sistema operacional | [CVE-2026-18508](https://avd.aquasec.com/nvd/cve-2026-18508) |
| Low | imagem (ubuntu 24.04) | libicu74 | 74.2-1ubuntu3.1 | sem correção | sistema operacional | [CVE-2025-5222](https://avd.aquasec.com/nvd/cve-2025-5222) |
| Low | imagem (ubuntu 24.04) | libsystemd0 | 255.4-1ubuntu8.18 | sem correção | sistema operacional | [CVE-2026-40228](https://avd.aquasec.com/nvd/cve-2026-40228) |
| Low | imagem (ubuntu 24.04) | libudev1 | 255.4-1ubuntu8.18 | sem correção | sistema operacional | [CVE-2026-40228](https://avd.aquasec.com/nvd/cve-2026-40228) |
| Low | imagem (ubuntu 24.04) | login | 1:4.13+dfsg1-4ubuntu3.2 | sem correção | sistema operacional | [CVE-2024-56433](https://avd.aquasec.com/nvd/cve-2024-56433) |
| Low | imagem (ubuntu 24.04) | passwd | 1:4.13+dfsg1-4ubuntu3.2 | sem correção | sistema operacional | [CVE-2024-56433](https://avd.aquasec.com/nvd/cve-2024-56433) |

## Configuração

Fonte: Trivy, sobre `Dockerfile`, manifestos Kubernetes e Terraform. Só controles reprovados.

| Severidade | Quantidade |
|---|---|
| High | 4 |
| Medium | 12 |
| Low | 13 |

| Severidade | Alvo | Pacote ou tipo | Versão | Corrigida em | Escopo ou descrição | Identificador |
|---|---|---|---|---|---|---|
| High | infra/main.tf | terraform | - | - | SSL connections to a SQL database instance should be enforced. | [GCP-0015](https://avd.aquasec.com/misconfig/gcp-0015) |
| High | infra/main.tf | terraform | - | - | Node metadata value disables metadata concealment. | [GCP-0057](https://avd.aquasec.com/misconfig/gcp-0057) |
| High | infra/main.tf | terraform | - | - | Master authorized networks should be configured on GKE clusters | [GCP-0061](https://avd.aquasec.com/misconfig/gcp-0061) |
| High | k8s/base/deployment.yaml | kubernetes | - | - | Default security context configured | [KSV-0118](https://avd.aquasec.com/misconfig/ksv-0118) |
| Medium | infra/main.tf | terraform | - | - | Temporary file logging should be enabled for all temporary files. | [GCP-0014](https://avd.aquasec.com/misconfig/gcp-0014) |
| Medium | infra/main.tf | terraform | - | - | Ensure that logging of connections is enabled. | [GCP-0016](https://avd.aquasec.com/misconfig/gcp-0016) |
| Medium | infra/main.tf | terraform | - | - | Ensure that logging of lock waits is enabled. | [GCP-0020](https://avd.aquasec.com/misconfig/gcp-0020) |
| Medium | infra/main.tf | terraform | - | - | Ensure that logging of disconnections is enabled. | [GCP-0022](https://avd.aquasec.com/misconfig/gcp-0022) |
| Medium | infra/main.tf | terraform | - | - | Enable automated backups to recover from data-loss | [GCP-0024](https://avd.aquasec.com/misconfig/gcp-0024) |
| Medium | infra/main.tf | terraform | - | - | Ensure that logging of checkpoints is enabled. | [GCP-0025](https://avd.aquasec.com/misconfig/gcp-0025) |
| Medium | infra/main.tf | terraform | - | - | Checks for service account defined for GKE nodes | [GCP-0050](https://avd.aquasec.com/misconfig/gcp-0050) |
| Medium | infra/main.tf | terraform | - | - | Network Policy should be enabled on GKE clusters | [GCP-0056](https://avd.aquasec.com/misconfig/gcp-0056) |
| Medium | infra/main.tf | terraform | - | - | Google Compute Subnetwork Logging Disabled | [GCP-0076](https://avd.aquasec.com/misconfig/gcp-0076) |
| Medium | infra/main.tf | terraform | - | - | Google Compute Subnetwork Logging Disabled | [GCP-0076](https://avd.aquasec.com/misconfig/gcp-0076) |
| Medium | k8s/base/deployment.yaml | kubernetes | - | - | Image tag ":latest" used | [KSV-0013](https://avd.aquasec.com/misconfig/ksv-0013) |
| Medium | k8s/base/deployment.yaml | kubernetes | - | - | Seccomp policies disabled | [KSV-0104](https://avd.aquasec.com/misconfig/ksv-0104) |
| Low | Dockerfile | dockerfile | - | - | No HEALTHCHECK defined | [DS-0026](https://avd.aquasec.com/misconfig/ds-0026) |
| Low | infra/main.tf | terraform | - | - | VPC flow logs should be enabled for all subnetworks | [GCP-0029](https://avd.aquasec.com/misconfig/gcp-0029) |
| Low | infra/main.tf | terraform | - | - | VPC flow logs should be enabled for all subnetworks | [GCP-0029](https://avd.aquasec.com/misconfig/gcp-0029) |
| Low | infra/main.tf | terraform | - | - | Clusters should have IP aliasing enabled | [GCP-0049](https://avd.aquasec.com/misconfig/gcp-0049) |
| Low | infra/main.tf | terraform | - | - | Clusters should be configured with Labels | [GCP-0051](https://avd.aquasec.com/misconfig/gcp-0051) |
| Low | infra/main.tf | terraform | - | - | Ensure Container-Optimized OS (cos) is used for Kubernetes Engine Clusters Node image | [GCP-0054](https://avd.aquasec.com/misconfig/gcp-0054) |
| Low | infra/main.tf | terraform | - | - | Kubernetes should have 'Automatic upgrade' enabled | [GCP-0058](https://avd.aquasec.com/misconfig/gcp-0058) |
| Low | infra/main.tf | terraform | - | - | Kubernetes should have 'Automatic repair' enabled | [GCP-0063](https://avd.aquasec.com/misconfig/gcp-0063) |
| Low | infra/main.tf | terraform | - | - | Google Compute Subnetwork with Private Google Access Disabled | [GCP-0075](https://avd.aquasec.com/misconfig/gcp-0075) |
| Low | k8s/base/deployment.yaml | kubernetes | - | - | Runs with UID <= 10000 | [KSV-0020](https://avd.aquasec.com/misconfig/ksv-0020) |
| Low | k8s/base/deployment.yaml | kubernetes | - | - | Runs with GID <= 10000 | [KSV-0021](https://avd.aquasec.com/misconfig/ksv-0021) |
| Low | k8s/base/deployment.yaml | kubernetes | - | - | Runtime/Default Seccomp profile not set | [KSV-0030](https://avd.aquasec.com/misconfig/ksv-0030) |
| Low | k8s/base/deployment.yaml | kubernetes | - | - | Workloads in the default namespace | [KSV-0110](https://avd.aquasec.com/misconfig/ksv-0110) |
