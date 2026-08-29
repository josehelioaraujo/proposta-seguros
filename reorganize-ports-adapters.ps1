# ============================================================
# reorganize-ports-adapters.ps1
# Reorganiza pastas para deixar explicito Ports/Output
# e Adapters/Output conforme Arquitetura Hexagonal
# ============================================================

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  Reorganizando Ports e Adapters"                            -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

# ── PROPOSTA SERVICE — DOMAIN ──
Write-Host "[1/4] PropostaService.Domain - Ports/Output..." -ForegroundColor Yellow

$portsProposta = "$root\src\PropostaService\PropostaService.Domain\Ports"
mkdir "$portsProposta\Output" -Force | Out-Null

Move-Item "$portsProposta\IPropostaRepository.cs" "$portsProposta\Output\" -Force
Move-Item "$portsProposta\IRegraSeguro.cs"        "$portsProposta\Output\" -Force

Write-Host "  IPropostaRepository.cs -> Ports/Output/" -ForegroundColor Green
Write-Host "  IRegraSeguro.cs        -> Ports/Output/" -ForegroundColor Green

# ── PROPOSTA SERVICE — INFRASTRUCTURE ──
Write-Host ""
Write-Host "[2/4] PropostaService.Infrastructure - Adapters/Output..." -ForegroundColor Yellow

$adaptersProposta = "$root\src\PropostaService\PropostaService.Infrastructure\Adapters"
mkdir "$adaptersProposta\Output" -Force | Out-Null

Move-Item "$adaptersProposta\InMemory" "$adaptersProposta\Output\" -Force
Move-Item "$adaptersProposta\Database" "$adaptersProposta\Output\" -Force

Write-Host "  InMemory/ -> Adapters/Output/InMemory/" -ForegroundColor Green
Write-Host "  Database/ -> Adapters/Output/Database/" -ForegroundColor Green

# ── CONTRATACAO SERVICE — DOMAIN ──
Write-Host ""
Write-Host "[3/4] ContratacaoService.Domain - Ports/Output..." -ForegroundColor Yellow

$portsContratacao = "$root\src\ContratacaoService\ContratacaoService.Domain\Ports"
mkdir "$portsContratacao\Output" -Force | Out-Null

Move-Item "$portsContratacao\IContratacaoRepository.cs" "$portsContratacao\Output\" -Force
Move-Item "$portsContratacao\IPropostaServiceClient.cs" "$portsContratacao\Output\" -Force

Write-Host "  IContratacaoRepository.cs -> Ports/Output/" -ForegroundColor Green
Write-Host "  IPropostaServiceClient.cs -> Ports/Output/" -ForegroundColor Green

# ── CONTRATACAO SERVICE — INFRASTRUCTURE ──
Write-Host ""
Write-Host "[4/4] ContratacaoService.Infrastructure - Adapters/Output..." -ForegroundColor Yellow

$adaptersContratacao = "$root\src\ContratacaoService\ContratacaoService.Infrastructure\Adapters"
mkdir "$adaptersContratacao\Output" -Force | Out-Null

Move-Item "$adaptersContratacao\InMemory" "$adaptersContratacao\Output\" -Force
Move-Item "$adaptersContratacao\Database" "$adaptersContratacao\Output\" -Force
Move-Item "$adaptersContratacao\Http"     "$adaptersContratacao\Output\" -Force

Write-Host "  InMemory/ -> Adapters/Output/InMemory/" -ForegroundColor Green
Write-Host "  Database/ -> Adapters/Output/Database/" -ForegroundColor Green
Write-Host "  Http/     -> Adapters/Output/Http/"     -ForegroundColor Green

# ── BUILD ──
Write-Host ""
Write-Host "[+] Verificando build..." -ForegroundColor Yellow
Set-Location $root
dotnet build .\proposta-seguros.sln

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  Reorganizacao concluida!"                                  -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Domain/Ports/Output/            -> Output Ports"           -ForegroundColor Gray
Write-Host "  Infrastructure/Adapters/Output/ -> Output Adapters"        -ForegroundColor Gray
Write-Host "  Api/                            -> Input Adapter (REST)"   -ForegroundColor Gray
Write-Host ""
