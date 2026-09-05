using DbUp;
using FluentValidation;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using PropostaService.Api.Mcp;
using PropostaService.Application.Extensions;
using PropostaService.Infrastructure.Extensions;

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

// Camadas â€” Hexagonal
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
        Description = "API para gerenciamento de propostas de seguro â€” Arquitetura Hexagonal"
    });
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// OpenTelemetry
var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
    ?? "http://jaeger:4317";

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("proposta-api"))
        .AddAspNetCoreInstrumentation(opts => { opts.RecordException = true; })
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(opts => { opts.Endpoint = new Uri(otlpEndpoint); }));

// MCP Server
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<PropostasMcpAdapter>();

// Health Checks
var usarBancoDados = builder.Configuration.GetValue<bool>("Features:UsarBancoDados");

var healthChecks = builder.Services.AddHealthChecks()
    .AddCheck("proposta-api", () =>
        HealthCheckResult.Healthy("Proposta de Seguros API â€” online"),
        tags: ["live"]);

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

app.UseHttpMetrics();
app.MapControllers();

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

// MCP endpoint
app.MapMcp("/mcp");
app.MapMcp("/sse");

app.MapMetrics();

// ── DbUp — Migrations automáticas ────────────────────────────────────────────
if (usarBancoDados)
{
    var connectionString = app.Configuration.GetConnectionString("DefaultConnection")!;
    var upgrader = DbUp.DeployChanges.To
        .PostgresqlDatabase(connectionString)
        .WithScriptsFromFileSystem(
            Path.Combine(AppContext.BaseDirectory, "migrations"),
            s => s.EndsWith(".sql"))
        .WithTransactionPerScript()
        .LogToConsole()
        .Build();

    var result = upgrader.PerformUpgrade();
    if (!result.Successful)
    {
        Console.WriteLine($"[DbUp] Falha ao aplicar migrations: {result.Error}");
        throw result.Error;
    }
}
app.Run();


