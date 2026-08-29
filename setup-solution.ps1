# ============================================================
# setup-solution.ps1
# Cria toda a estrutura de pastas e projetos .NET 8
# Sistema de Proposta de Seguros — Arquitetura Hexagonal
# ============================================================
# Uso: .\setup-solution.ps1
# Executar a partir de: C:\Git\proposta-seguros
# ============================================================

$ErrorActionPreference = "Stop"
$root   = $PSScriptRoot
$src    = "$root\src"
$tests  = "$root\tests"
$sln    = "proposta-seguros"

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  Proposta de Seguros - Setup da Solution"                   -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

Set-Location $root

# ------------------------------------------------------------
# 1. SOLUTION
# ------------------------------------------------------------
Write-Host "[1/6] Criando Solution..." -ForegroundColor Yellow
dotnet new sln -n $sln --force | Out-Null
Write-Host "  + $sln.sln" -ForegroundColor Green

# ------------------------------------------------------------
# 2. PROJETOS — PROPOSTA SERVICE
# ------------------------------------------------------------
Write-Host ""
Write-Host "[2/6] Criando projetos PropostaService..." -ForegroundColor Yellow

dotnet new classlib -n "PropostaService.Domain"         -o "$src\PropostaService\PropostaService.Domain"         --no-restore | Out-Null; Write-Host "  + PropostaService.Domain"         -ForegroundColor Green
dotnet new classlib -n "PropostaService.Application"    -o "$src\PropostaService\PropostaService.Application"    --no-restore | Out-Null; Write-Host "  + PropostaService.Application"    -ForegroundColor Green
dotnet new classlib -n "PropostaService.Infrastructure" -o "$src\PropostaService\PropostaService.Infrastructure" --no-restore | Out-Null; Write-Host "  + PropostaService.Infrastructure" -ForegroundColor Green
dotnet new webapi   -n "PropostaService.Api"            -o "$src\PropostaService\PropostaService.Api"            --no-restore | Out-Null; Write-Host "  + PropostaService.Api"            -ForegroundColor Green

# ------------------------------------------------------------
# 3. PROJETOS — CONTRATACAO SERVICE
# ------------------------------------------------------------
Write-Host ""
Write-Host "[3/6] Criando projetos ContratacaoService..." -ForegroundColor Yellow

dotnet new classlib -n "ContratacaoService.Domain"         -o "$src\ContratacaoService\ContratacaoService.Domain"         --no-restore | Out-Null; Write-Host "  + ContratacaoService.Domain"         -ForegroundColor Green
dotnet new classlib -n "ContratacaoService.Application"    -o "$src\ContratacaoService\ContratacaoService.Application"    --no-restore | Out-Null; Write-Host "  + ContratacaoService.Application"    -ForegroundColor Green
dotnet new classlib -n "ContratacaoService.Infrastructure" -o "$src\ContratacaoService\ContratacaoService.Infrastructure" --no-restore | Out-Null; Write-Host "  + ContratacaoService.Infrastructure" -ForegroundColor Green
dotnet new webapi   -n "ContratacaoService.Api"            -o "$src\ContratacaoService\ContratacaoService.Api"            --no-restore | Out-Null; Write-Host "  + ContratacaoService.Api"            -ForegroundColor Green

# ------------------------------------------------------------
# 4. PROJETOS — TESTES
# ------------------------------------------------------------
Write-Host ""
Write-Host "[4/6] Criando projetos de Testes..." -ForegroundColor Yellow

dotnet new xunit -n "PropostaService.Tests"    -o "$tests\PropostaService.Tests"    --no-restore | Out-Null; Write-Host "  + PropostaService.Tests"    -ForegroundColor Green
dotnet new xunit -n "ContratacaoService.Tests" -o "$tests\ContratacaoService.Tests" --no-restore | Out-Null; Write-Host "  + ContratacaoService.Tests" -ForegroundColor Green

# ------------------------------------------------------------
# 5. ADICIONAR PROJETOS NA SOLUTION
# ------------------------------------------------------------
Write-Host ""
Write-Host "[5/6] Adicionando projetos na Solution..." -ForegroundColor Yellow

dotnet sln add "$src\PropostaService\PropostaService.Domain\PropostaService.Domain.csproj"                 | Out-Null; Write-Host "  + PropostaService.Domain"         -ForegroundColor Green
dotnet sln add "$src\PropostaService\PropostaService.Application\PropostaService.Application.csproj"       | Out-Null; Write-Host "  + PropostaService.Application"    -ForegroundColor Green
dotnet sln add "$src\PropostaService\PropostaService.Infrastructure\PropostaService.Infrastructure.csproj" | Out-Null; Write-Host "  + PropostaService.Infrastructure" -ForegroundColor Green
dotnet sln add "$src\PropostaService\PropostaService.Api\PropostaService.Api.csproj"                       | Out-Null; Write-Host "  + PropostaService.Api"            -ForegroundColor Green
dotnet sln add "$src\ContratacaoService\ContratacaoService.Domain\ContratacaoService.Domain.csproj"                 | Out-Null; Write-Host "  + ContratacaoService.Domain"         -ForegroundColor Green
dotnet sln add "$src\ContratacaoService\ContratacaoService.Application\ContratacaoService.Application.csproj"       | Out-Null; Write-Host "  + ContratacaoService.Application"    -ForegroundColor Green
dotnet sln add "$src\ContratacaoService\ContratacaoService.Infrastructure\ContratacaoService.Infrastructure.csproj" | Out-Null; Write-Host "  + ContratacaoService.Infrastructure" -ForegroundColor Green
dotnet sln add "$src\ContratacaoService\ContratacaoService.Api\ContratacaoService.Api.csproj"                       | Out-Null; Write-Host "  + ContratacaoService.Api"            -ForegroundColor Green
dotnet sln add "$tests\PropostaService.Tests\PropostaService.Tests.csproj"                                 | Out-Null; Write-Host "  + PropostaService.Tests"    -ForegroundColor Green
dotnet sln add "$tests\ContratacaoService.Tests\ContratacaoService.Tests.csproj"                           | Out-Null; Write-Host "  + ContratacaoService.Tests" -ForegroundColor Green

# ------------------------------------------------------------
# 6. REFERENCIAS ENTRE PROJETOS
# ------------------------------------------------------------
Write-Host ""
Write-Host "[6/6] Configurando referencias entre projetos..." -ForegroundColor Yellow

Write-Host "  PropostaService:" -ForegroundColor Cyan
dotnet add "$src\PropostaService\PropostaService.Application\PropostaService.Application.csproj"    reference "$src\PropostaService\PropostaService.Domain\PropostaService.Domain.csproj"          | Out-Null; Write-Host "    Application    -> Domain"         -ForegroundColor Green
dotnet add "$src\PropostaService\PropostaService.Infrastructure\PropostaService.Infrastructure.csproj" reference "$src\PropostaService\PropostaService.Domain\PropostaService.Domain.csproj"      | Out-Null; Write-Host "    Infrastructure -> Domain"         -ForegroundColor Green
dotnet add "$src\PropostaService\PropostaService.Api\PropostaService.Api.csproj"                    reference "$src\PropostaService\PropostaService.Application\PropostaService.Application.csproj"    | Out-Null; Write-Host "    Api            -> Application"    -ForegroundColor Green
dotnet add "$src\PropostaService\PropostaService.Api\PropostaService.Api.csproj"                    reference "$src\PropostaService\PropostaService.Infrastructure\PropostaService.Infrastructure.csproj" | Out-Null; Write-Host "    Api            -> Infrastructure" -ForegroundColor Green

Write-Host "  ContratacaoService:" -ForegroundColor Cyan
dotnet add "$src\ContratacaoService\ContratacaoService.Application\ContratacaoService.Application.csproj"    reference "$src\ContratacaoService\ContratacaoService.Domain\ContratacaoService.Domain.csproj"          | Out-Null; Write-Host "    Application    -> Domain"         -ForegroundColor Green
dotnet add "$src\ContratacaoService\ContratacaoService.Infrastructure\ContratacaoService.Infrastructure.csproj" reference "$src\ContratacaoService\ContratacaoService.Domain\ContratacaoService.Domain.csproj"      | Out-Null; Write-Host "    Infrastructure -> Domain"         -ForegroundColor Green
dotnet add "$src\ContratacaoService\ContratacaoService.Api\ContratacaoService.Api.csproj"                    reference "$src\ContratacaoService\ContratacaoService.Application\ContratacaoService.Application.csproj"    | Out-Null; Write-Host "    Api            -> Application"    -ForegroundColor Green
dotnet add "$src\ContratacaoService\ContratacaoService.Api\ContratacaoService.Api.csproj"                    reference "$src\ContratacaoService\ContratacaoService.Infrastructure\ContratacaoService.Infrastructure.csproj" | Out-Null; Write-Host "    Api            -> Infrastructure" -ForegroundColor Green

Write-Host "  Tests:" -ForegroundColor Cyan
dotnet add "$tests\PropostaService.Tests\PropostaService.Tests.csproj"    reference "$src\PropostaService\PropostaService.Application\PropostaService.Application.csproj" | Out-Null
dotnet add "$tests\PropostaService.Tests\PropostaService.Tests.csproj"    reference "$src\PropostaService\PropostaService.Domain\PropostaService.Domain.csproj"           | Out-Null; Write-Host "    PropostaService.Tests    -> Application + Domain" -ForegroundColor Green
dotnet add "$tests\ContratacaoService.Tests\ContratacaoService.Tests.csproj" reference "$src\ContratacaoService\ContratacaoService.Application\ContratacaoService.Application.csproj" | Out-Null
dotnet add "$tests\ContratacaoService.Tests\ContratacaoService.Tests.csproj" reference "$src\ContratacaoService\ContratacaoService.Domain\ContratacaoService.Domain.csproj"           | Out-Null; Write-Host "    ContratacaoService.Tests -> Application + Domain" -ForegroundColor Green

# ------------------------------------------------------------
# NUGET PACKAGES
# ------------------------------------------------------------
Write-Host ""
Write-Host "[+] Instalando NuGet packages..." -ForegroundColor Yellow

$infraProposta    = "$src\PropostaService\PropostaService.Infrastructure\PropostaService.Infrastructure.csproj"
$appProposta      = "$src\PropostaService\PropostaService.Application\PropostaService.Application.csproj"
$apiProposta      = "$src\PropostaService\PropostaService.Api\PropostaService.Api.csproj"
$infraContratacao = "$src\ContratacaoService\ContratacaoService.Infrastructure\ContratacaoService.Infrastructure.csproj"
$appContratacao   = "$src\ContratacaoService\ContratacaoService.Application\ContratacaoService.Application.csproj"
$apiContratacao   = "$src\ContratacaoService\ContratacaoService.Api\ContratacaoService.Api.csproj"
$testsProposta    = "$tests\PropostaService.Tests\PropostaService.Tests.csproj"
$testsContratacao = "$tests\ContratacaoService.Tests\ContratacaoService.Tests.csproj"

Write-Host "  PropostaService.Infrastructure:" -ForegroundColor Cyan
dotnet add $infraProposta package Dapper | Out-Null; Write-Host "    + Dapper" -ForegroundColor Green
dotnet add $infraProposta package Npgsql | Out-Null; Write-Host "    + Npgsql" -ForegroundColor Green

Write-Host "  PropostaService.Application:" -ForegroundColor Cyan
dotnet add $appProposta package FluentValidation | Out-Null; Write-Host "    + FluentValidation" -ForegroundColor Green

Write-Host "  PropostaService.Api:" -ForegroundColor Cyan
dotnet add $apiProposta package FluentValidation.AspNetCore | Out-Null; Write-Host "    + FluentValidation.AspNetCore" -ForegroundColor Green
dotnet add $apiProposta package Swashbuckle.AspNetCore       | Out-Null; Write-Host "    + Swashbuckle.AspNetCore"      -ForegroundColor Green

Write-Host "  ContratacaoService.Infrastructure:" -ForegroundColor Cyan
dotnet add $infraContratacao package Dapper | Out-Null; Write-Host "    + Dapper" -ForegroundColor Green
dotnet add $infraContratacao package Npgsql | Out-Null; Write-Host "    + Npgsql" -ForegroundColor Green

Write-Host "  ContratacaoService.Application:" -ForegroundColor Cyan
dotnet add $appContratacao package FluentValidation | Out-Null; Write-Host "    + FluentValidation" -ForegroundColor Green

Write-Host "  ContratacaoService.Api:" -ForegroundColor Cyan
dotnet add $apiContratacao package FluentValidation.AspNetCore | Out-Null; Write-Host "    + FluentValidation.AspNetCore" -ForegroundColor Green
dotnet add $apiContratacao package Swashbuckle.AspNetCore       | Out-Null; Write-Host "    + Swashbuckle.AspNetCore"      -ForegroundColor Green

Write-Host "  Tests:" -ForegroundColor Cyan
dotnet add $testsProposta    package Moq              | Out-Null
dotnet add $testsProposta    package FluentAssertions | Out-Null
dotnet add $testsContratacao package Moq              | Out-Null
dotnet add $testsContratacao package FluentAssertions | Out-Null
Write-Host "    + Moq + FluentAssertions (ambos)" -ForegroundColor Green

# ------------------------------------------------------------
# RESTORE + GITIGNORE
# ------------------------------------------------------------
Write-Host ""
Write-Host "[+] Restaurando packages..." -ForegroundColor Yellow
dotnet restore | Out-Null
Write-Host "  Restauracao concluida!" -ForegroundColor Green

Write-Host ""
Write-Host "[+] Criando .gitignore..." -ForegroundColor Yellow
dotnet new gitignore --force | Out-Null
Write-Host "  + .gitignore" -ForegroundColor Green

# ------------------------------------------------------------
# RESUMO
# ------------------------------------------------------------
Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  Setup concluido com sucesso!"                              -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Proximo passo:" -ForegroundColor Yellow
Write-Host "  Abrir proposta-seguros.sln no Visual Studio 2022" -ForegroundColor Yellow
Write-Host ""
