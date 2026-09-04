using FluentValidation;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;
using PropostaService.Application.Extensions;
using PropostaService.Infrastructure.Extensions;

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

// Camadas — Hexagonal
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "Proposta de Seguros API",
        Version     = "v1",
        Description = "API para gerenciamento de propostas de seguro — Arquitetura Hexagonal"
    });
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Health Checks
var usarBancoDados = builder.Configuration.GetValue<bool>("Features:UsarBancoDados");

var healthChecks = builder.Services.AddHealthChecks()
    .AddCheck("proposta-api", () =>
        HealthCheckResult.Healthy("Proposta de Seguros API — online"),
        tags: ["live"]);

// Postgres — so monitora se banco estiver habilitado
if (usarBancoDados)
{
    healthChecks.AddNpgSql(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
        name:             "postgres",
        tags:             ["ready", "db"]);
}

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Proposta de Seguros API v1");
    c.RoutePrefix = string.Empty;
});

// Prometheus — coleta metricas HTTP automaticamente
app.UseHttpMetrics();

app.MapControllers();

// Health Check Endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate      = check => check.Tags.Contains("live"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate      = check => check.Tags.Contains("ready") || check.Tags.Contains("db"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Prometheus — endpoint /metrics
app.MapMetrics();

app.Run();
