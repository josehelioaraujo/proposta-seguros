using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Xunit;

namespace PropostaService.IntegrationTests.Fixtures;

public class PostgresApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    public static bool DockerDisponivel { get; private set; }

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("seguros_db")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    static PostgresApiFixture()
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
        await _postgres.StartAsync();
        await AplicarMigrationsAsync();
    }

    public new async Task DisposeAsync()
    {
        if (!DockerDisponivel)
        {
            await base.DisposeAsync();
            return;
        }
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Features:UsarBancoDados"]              = "true",
                ["Features:UsarRabbitMQ"]                = "false",
                ["ConnectionStrings:DefaultConnection"]  = _postgres.GetConnectionString()
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
            "V002__create_table_propostas.sql"
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
