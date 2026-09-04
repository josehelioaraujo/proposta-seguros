# ==============================================================================
# apply-observability-v2.ps1
# Jaeger + OpenTelemetry + Loki + Promtail
# Extrair ZIP em %USERPROFILE%\Downloads\proposta-seguros-obs-v2\
# Executar na raiz do repositorio: .\apply-observability-v2.ps1
# ==============================================================================

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot = (Get-Location).Path
$ZipRoot  = Join-Path $env:USERPROFILE "Downloads\proposta-seguros-obs-v2"

function Write-Step { param($n,$msg) Write-Host "`n[$n] $msg" -ForegroundColor Cyan }
function Write-Ok   { param($msg)    Write-Host "    OK  $msg" -ForegroundColor Green }
function Write-Skip { param($msg)    Write-Host "    --  $msg" -ForegroundColor DarkGray }
function Write-Warn { param($msg)    Write-Host "    !   $msg" -ForegroundColor Yellow }

function Copy-Safe {
    param($Src, $Dst)
    $dir = Split-Path $Dst -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    Copy-Item -Path $Src -Destination $Dst -Force
}

function Apply-LF {
    param($Path)
    $txt = [System.IO.File]::ReadAllText($Path) -replace "`r`n", "`n"
    [System.IO.File]::WriteAllText($Path, $txt, [System.Text.Encoding]::UTF8)
}

if (-not (Test-Path (Join-Path $RepoRoot "docker-compose.yml"))) {
    Write-Host "`n[ERRO] Execute na raiz do repositorio" -ForegroundColor Red; exit 1
}
if (-not (Test-Path $ZipRoot)) {
    Write-Host "`n[ERRO] Pasta nao encontrada: $ZipRoot" -ForegroundColor Red
    Write-Host "       Extraia o ZIP em Downloads primeiro." -ForegroundColor Yellow; exit 1
}

Write-Host ""
Write-Host "======================================================================" -ForegroundColor Cyan
Write-Host "  Observabilidade v2 — Jaeger + OpenTelemetry + Loki + Promtail" -ForegroundColor Cyan
Write-Host "======================================================================" -ForegroundColor Cyan

# ETAPA 1 — NuGet OpenTelemetry
Write-Step 1 "Adicionando pacotes OpenTelemetry"

$apiProjects = @(
    "src\PropostaService\PropostaService.Api\PropostaService.Api.csproj",
    "src\ContratacaoService\ContratacaoService.Api\ContratacaoService.Api.csproj"
)

$packages = @(
    "OpenTelemetry.Extensions.Hosting",
    "OpenTelemetry.Instrumentation.AspNetCore",
    "OpenTelemetry.Instrumentation.Http",
    "OpenTelemetry.Exporter.OpenTelemetryProtocol"
)

foreach ($rel in $apiProjects) {
    $full = Join-Path $RepoRoot $rel
    $content = Get-Content $full -Raw
    foreach ($pkg in $packages) {
        if ($content -match [regex]::Escape($pkg)) {
            Write-Skip "$pkg ja presente em $rel"
        } else {
            dotnet add $full package $pkg | Out-Null
            Write-Ok "$pkg → $rel"
        }
    }
}

# ETAPA 2 — Copiar arquivos
Write-Step 2 "Copiando arquivos"

$files = @(
    "docker-compose.yml",
    "observability\loki\loki.yml",
    "observability\promtail\promtail.yml",
    "observability\grafana\datasources\prometheus.yml",
    "observability\grafana\dashboards\dashboard.yml",
    "observability\grafana\dashboards\proposta-seguros.json",
    "src\PropostaService\PropostaService.Api\Program.cs",
    "src\ContratacaoService\ContratacaoService.Api\Program.cs",
    "scripts\set-monitoring.sh"
)

foreach ($rel in $files) {
    $src = Join-Path $ZipRoot $rel
    $dst = Join-Path $RepoRoot $rel
    if (Test-Path $src) { Copy-Safe $src $dst; Write-Ok $rel }
    else                { Write-Warn "Nao encontrado: $rel" }
}

# ETAPA 3 — LF nos scripts
Write-Step 3 "Aplicando LF nos scripts .sh"
Apply-LF (Join-Path $RepoRoot "scripts\set-monitoring.sh")
Write-Ok "scripts\set-monitoring.sh"

# ETAPA 4 — Build
Write-Step 4 "Build de verificacao"

$sln = Get-ChildItem -Path $RepoRoot -Filter "*.sln" | Select-Object -First 1
dotnet build $sln.FullName -v q --nologo 2>&1 | Where-Object { $_ -match "error|Build succeeded" }
if ($LASTEXITCODE -eq 0) { Write-Ok "Build passou!" }
else                      { Write-Warn "Build com erros — verifique acima" }

Write-Host ""
Write-Host "======================================================================" -ForegroundColor Green
Write-Host "  Concluido — Proximos passos:" -ForegroundColor Green
Write-Host "======================================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  1. Rodar testes:" -ForegroundColor Yellow
Write-Host "     dotnet test proposta-seguros.sln --filter 'FullyQualifiedName!~IntegrationTests'" -ForegroundColor White
Write-Host ""
Write-Host "  2. Commit e push:" -ForegroundColor Yellow
Write-Host "     git add ." -ForegroundColor White
Write-Host "     git commit -m 'feat: add Jaeger tracing + Loki logs observability stack'" -ForegroundColor White
Write-Host "     git push" -ForegroundColor White
Write-Host ""
Write-Host "  3. Apos o deploy, acessar:" -ForegroundColor Yellow
Write-Host "     Grafana    : https://grafana.2.25.122.11.nip.io" -ForegroundColor White
Write-Host "     Jaeger     : http://2.25.122.11:16686" -ForegroundColor White
Write-Host "     Prometheus : http://2.25.122.11:9090" -ForegroundColor White
Write-Host "     Loki       : http://2.25.122.11:3100" -ForegroundColor White
Write-Host ""
