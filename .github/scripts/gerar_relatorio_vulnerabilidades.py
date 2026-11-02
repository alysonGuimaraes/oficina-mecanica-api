"""Gera o relatório de vulnerabilidades em Markdown, com três fontes:

1. Dependências NuGet: saída JSON de
   `dotnet list <solução> package --vulnerable --include-transitive --format json`.
2. Imagem Docker: saída JSON de `trivy image --format json`.
3. Configuração: saída JSON de `trivy config --format json` (Dockerfile, Kubernetes e Terraform).

Uso:
    python gerar_relatorio_vulnerabilidades.py --nuget audit.json --imagem trivy-image.json \
        --config trivy-config.json --saida <arquivo.md> --commit <sha>
"""

import argparse
import json
import sys
from datetime import datetime, timezone

# Severidades normalizadas. O NuGet usa "Moderate" e o Trivy usa "MEDIUM": ambos viram "Medium".
SEVERITY_ORDER = {"Critical": 0, "High": 1, "Medium": 2, "Low": 3, "Unknown": 4}
SEVERITY_NAMES = {
    "CRITICAL": "Critical",
    "HIGH": "High",
    "MEDIUM": "Medium",
    "MODERATE": "Medium",
    "LOW": "Low",
}


def normalize_severity(value):
    return SEVERITY_NAMES.get((value or "").upper(), "Unknown")


def sort_key(row):
    return (SEVERITY_ORDER.get(row["severity"], 9), row["target"], row["package"])


def collect_nuget(audit):
    rows = []
    for project in audit.get("projects", []):
        target = project["path"].replace("\\", "/").split("/")[-1]
        # Projetos sem pacotes vulneráveis não trazem a chave "frameworks".
        for framework in project.get("frameworks", []):
            for kind, scope in (("topLevelPackages", "direta"), ("transitivePackages", "transitiva")):
                for package in framework.get(kind, []):
                    for vulnerability in package.get("vulnerabilities", []):
                        rows.append({
                            "severity": normalize_severity(vulnerability["severity"]),
                            "target": target,
                            "package": package["id"],
                            "version": package["resolvedVersion"],
                            "fixed": "-",
                            "scope": scope,
                            "identifier": vulnerability["advisoryurl"].rstrip("/").split("/")[-1],
                            "link": vulnerability["advisoryurl"],
                        })
    return sorted(rows, key=sort_key)


def display_image_target(target):
    # O Trivy informa o caminho do arquivo da imagem e a distribuição entre parênteses,
    # por exemplo "/work/image.tar (ubuntu 24.04)". Só a distribuição interessa no relatório.
    if target.endswith(")") and "(" in target:
        return "imagem " + target[target.index("("):]
    return target


def collect_image(trivy):
    rows = []
    for result in trivy.get("Results") or []:
        for vulnerability in result.get("Vulnerabilities") or []:
            identifier = vulnerability["VulnerabilityID"]
            rows.append({
                "severity": normalize_severity(vulnerability.get("Severity")),
                "target": display_image_target(result.get("Target", "-")),
                "package": vulnerability.get("PkgName", "-"),
                "version": vulnerability.get("InstalledVersion", "-"),
                "fixed": vulnerability.get("FixedVersion") or "sem correção",
                "scope": "sistema operacional" if result.get("Class") == "os-pkgs" else "biblioteca",
                "identifier": identifier,
                "link": vulnerability.get("PrimaryURL") or "",
            })
    return sorted(rows, key=sort_key)


def collect_config(trivy):
    rows = []
    for result in trivy.get("Results") or []:
        for finding in result.get("Misconfigurations") or []:
            # Só entram os controles reprovados. Os aprovados não são achados.
            if finding.get("Status") != "FAIL":
                continue
            rows.append({
                "severity": normalize_severity(finding.get("Severity")),
                "target": result.get("Target", "-"),
                "package": result.get("Type", "-"),
                "version": "-",
                "fixed": "-",
                "scope": finding.get("Title", "-"),
                "identifier": finding.get("ID", "-"),
                "link": finding.get("PrimaryURL") or "",
            })
    return sorted(rows, key=sort_key)


def render_summary(rows):
    counts = {}
    for row in rows:
        counts[row["severity"]] = counts.get(row["severity"], 0) + 1

    lines = ["| Severidade | Quantidade |", "|---|---|"]
    for severity in sorted(counts, key=lambda s: SEVERITY_ORDER.get(s, 9)):
        lines.append(f"| {severity} | {counts[severity]} |")
    return lines


def render_detail(rows):
    lines = [
        "| Severidade | Alvo | Pacote ou tipo | Versão | Corrigida em | Escopo ou descrição | Identificador |",
        "|---|---|---|---|---|---|---|",
    ]
    for row in rows:
        identifier = f"[{row['identifier']}]({row['link']})" if row["link"] else row["identifier"]
        lines.append(
            f"| {row['severity']} | {row['target']} | {row['package']} | {row['version']} "
            f"| {row['fixed']} | {row['scope']} | {identifier} |"
        )
    return lines


def render_section(title, source_description, rows):
    lines = [f"## {title}", "", source_description, ""]
    if not rows:
        lines += ["Nenhum achado.", ""]
        return lines

    lines += render_summary(rows) + [""]
    lines += render_detail(rows) + [""]
    return lines


def render(nuget_rows, image_rows, config_rows, commit, generated_at):
    lines = [
        "# Relatório de vulnerabilidades",
        "",
        f"- Gerado em: {generated_at:%Y-%m-%d %H:%M} UTC",
        f"- Commit analisado: `{commit}`",
        "",
        "Arquivo gerado automaticamente pelo workflow `security-report.yml` a cada push na `main`. Não edite manualmente.",
        "",
    ]
    lines += render_section(
        "Dependências NuGet",
        "Fonte: `dotnet list package --vulnerable --include-transitive`, com a base de advisories do NuGet.",
        nuget_rows,
    )
    lines += render_section(
        "Imagem Docker",
        "Fonte: Trivy, sobre a imagem construída a partir do `Dockerfile` (sistema operacional e bibliotecas do runtime).",
        image_rows,
    )
    lines += render_section(
        "Configuração",
        "Fonte: Trivy, sobre `Dockerfile`, manifestos Kubernetes e Terraform. Só controles reprovados.",
        config_rows,
    )
    return "\n".join(lines)


def load(path):
    with open(path, encoding="utf-8") as file:
        return json.load(file)


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--nuget", required=True, help="JSON do dotnet list package --vulnerable")
    parser.add_argument("--imagem", required=True, help="JSON do trivy image")
    parser.add_argument("--config", required=True, help="JSON do trivy config")
    parser.add_argument("--saida", required=True, help="arquivo Markdown de saída")
    parser.add_argument("--commit", required=True, help="commit analisado")
    args = parser.parse_args()

    report = render(
        collect_nuget(load(args.nuget)),
        collect_image(load(args.imagem)),
        collect_config(load(args.config)),
        args.commit,
        datetime.now(timezone.utc),
    )

    with open(args.saida, "w", encoding="utf-8", newline="\n") as file:
        file.write(report)

    print(f"Relatório gravado em {args.saida}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
