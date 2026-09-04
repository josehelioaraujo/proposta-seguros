using FluentValidation;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;
using ContratacaoService.Application.Extensions;
using ContratacaoService.Infrastructure.Extensions;

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
        Title       = "Contratacao de Seguros API",
        Version     = "v1",
        Description = "API para contratacao de propostas de seguro — Arquitetura Hexagonal"
    });
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Health Checks
var usarBancoDados = builder.Configuration.GetValue<bool>("Features:UsarBancoDados");
var usarRabbitMQ   = builder.Configuration.GetValue<bool>("Features:UsarRabbitMQ");

var healthChecks = builder.Services.AddHealthChecks()
    .AddCheck("contratacao-api", () =>
        HealthCheckResult.Healthy("Contratacao de Seguros API — online"),
        tags: ["live"])
    .AddUrlGroup(
        uri:  new Uri((builder.Configuration["Services:PropostaService"]
              ?? "http://localhost:5001") + "/health/live"),
        name: "proposta-service",
        tags: ["ready", "external"]);

// Postgres — so monitora se banco estiver habilitado
if (usarBancoDados)
{
    healthChecks.AddNpgSql(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!,
        name:             "postgres",
        tags:             ["ready", "db"]);
}

// RabbitMQ — so monitora se estiver habilitado
if (usarRabbitMQ)
{
    healthChecks.AddCheck("rabbitmq", () =>
        HealthCheckResult.Healthy("RabbitMQ — online"),
        tags: ["ready", "mq"]);
}

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Contratacao de Seguros API v1");
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
    Predicate      = check => check.Tags.Contains("ready") || check.Tags.Contains("db") || check.Tags.Contains("mq"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Prometheus — endpoint /metrics
app.MapMetrics();

app.Run();
