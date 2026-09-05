using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.Kafka;
using Testcontainers.PostgreSql;
using Xunit;

namespace ContratacaoService.IntegrationTests.Fixtures;

public class KafkaFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    public static bool DockerDisponivel { get; private set; }

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("seguros_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly KafkaContainer _kafka = new KafkaBuilder()
        .WithImage("confluentinc/cp-kafka:7.4.0")
        .Build();

    public string KafkaBootstrapServers => _kafka.GetBootstrapAddress();

    static KafkaFixture()
    {
        try
        {
            var socketPath = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? @"\\.\pipe\docker_engine"
                : "/var/run/docker.sock";

            DockerDisponivel = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? System.Diagnostics.Process.GetProcessesByName("com.docker.backend").Length > 0 ||
                  System.Diagnostics.Process.GetProcessesByName("dockerd").Length > 0
                : File.Exists(socketPath);
        }
        catch { DockerDisponivel = false; }
    }

    public async Task InitializeAsync()
    {
        if (!DockerDisponivel) return;
        await Task.WhenAll(_postgres.StartAsync(), _kafka.StartAsync());
        await AplicarMigrationsAsync();
    }

    public new async Task DisposeAsync()
    {
        if (!DockerDisponivel) { await base.DisposeAsync(); return; }
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _kafka.DisposeAsync().AsTask());
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Features:UsarBancoDados"]               = "true",
                ["Features:UsarKafka"]                    = "true",
                ["Features:UsarRabbitMQ"]                 = "false",
                ["ConnectionStrings:DefaultConnection"]    = _postgres.GetConnectionString(),
                ["Kafka:BootstrapServers"]                = _kafka.GetBootstrapAddress(),
                ["Kafka:Topicos:PropostaContratadaEvent"] = "proposta-contratada",
                ["Services:PropostaService"]              = "http://localhost:5001"
            });
        });
    }

    private async Task AplicarMigrationsAsync()
    {
        var migrationsPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "..", "migrations");

        var scripts = new[]
        {
            "V001__create_schema_proposta.sql",
            "V002__create_table_propostas.sql",
            "V003__create_schema_contratacao.sql",
            "V004__create_table_contratacoes.sql",
            "V005__add_criado_em_contratacoes.sql",
            "V006__create_table_outbox.sql"
        };

        using var conn = new Npgsql.NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();

        foreach (var script in scripts)
        {
            var file = Path.Combine(migrationsPath, script);
            if (!File.Exists(file)) continue;
            var sql = await File.ReadAllTextAsync(file);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
