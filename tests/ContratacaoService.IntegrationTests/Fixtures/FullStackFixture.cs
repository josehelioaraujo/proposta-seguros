using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Xunit;

namespace ContratacaoService.IntegrationTests.Fixtures;

public class FullStackFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    public static bool DockerDisponivel { get; private set; }

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("seguros_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder()
        .WithImage("rabbitmq:4-management-alpine")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    public string RabbitMqConnectionString => _rabbitMq.GetConnectionString();

    static FullStackFixture()
    {
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var client = new Docker.DotNet.DockerClientConfiguration().CreateClient();
            client.System.PingAsync(cts.Token).GetAwaiter().GetResult();
            DockerDisponivel = true;
        }
        catch
        {
            DockerDisponivel = false;
        }
    }

    public async Task InitializeAsync()
    {
        if (!DockerDisponivel) return;

        await Task.WhenAll(_postgres.StartAsync(), _rabbitMq.StartAsync());
        await AplicarMigrationsAsync();
    }

    public new async Task DisposeAsync()
    {
        if (!DockerDisponivel)
        {
            await base.DisposeAsync();
            return;
        }
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _rabbitMq.DisposeAsync().AsTask());
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var uri = new Uri(_rabbitMq.GetConnectionString());
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Features:UsarBancoDados"]             = "true",
                ["Features:UsarRabbitMQ"]               = "true",
                ["ConnectionStrings:DefaultConnection"]  = _postgres.GetConnectionString(),
                ["RabbitMQ:Host"]                       = uri.Host,
                ["RabbitMQ:Port"]                       = uri.Port.ToString(),
                ["RabbitMQ:Username"]                   = "guest",
                ["RabbitMQ:Password"]                   = "guest",
                ["Services:PropostaService"]            = "http://localhost:5001"
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
            "V004__create_table_contratacoes.sql"
        };

        using var conn = new Npgsql.NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();

        foreach (var script in scripts)
        {
            var file = Path.Combine(migrationsPath, script);
            if (File.Exists(file))
            {
                var sql = await File.ReadAllTextAsync(file);
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
