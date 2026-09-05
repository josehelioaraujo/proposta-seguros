using System.Net.Http.Json;
using Confluent.Kafka;
using FluentAssertions;
using ContratacaoService.IntegrationTests.Fixtures;
using Xunit;
using Microsoft.Extensions.Configuration;

namespace ContratacaoService.IntegrationTests;

public class ContratacaoKafkaTests : IClassFixture<KafkaFixture>
{
    private readonly HttpClient   _client;
    private readonly KafkaFixture _fixture;

    private static readonly bool _podeRodar =
        Environment.GetEnvironmentVariable("RUN_INTEGRATION_TESTS") == "true"
        && KafkaFixture.DockerDisponivel;

    public ContratacaoKafkaTests(KafkaFixture fixture)
    {
        _fixture = fixture;
        _client  = fixture.CreateClient();
    }

    [SkippableFact]
    public async Task Kafka_DeveEstarAcessivel()
    {
        Skip.IfNot(_podeRodar, "Testes Kafka ignorados -- Docker indisponivel ou RUN_INTEGRATION_TESTS != true");

        var config = new AdminClientConfig { BootstrapServers = _fixture.KafkaBootstrapServers };
        using var admin = new AdminClientBuilder(config).Build();

        var meta = admin.GetMetadata(TimeSpan.FromSeconds(10));
        meta.Should().NotBeNull();
        meta.Brokers.Should().NotBeEmpty();
    }

    [SkippableFact]
    public async Task ContratarProposta_DevePublicarEventoNoKafka()
    {
        Skip.IfNot(_podeRodar, "Testes Kafka ignorados -- Docker indisponivel ou RUN_INTEGRATION_TESTS != true");

        var propostaId = Guid.NewGuid();
        var cpf        = "529.982.247-25";

        await InserirPropostaAprovadaAsync(propostaId, cpf);

        var payload  = new { propostaId, cpf };
        var response = await _client.PostAsJsonAsync("/api/Contratacoes", payload);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        await Task.Delay(TimeSpan.FromSeconds(8));

        var config = new ConsumerConfig
        {
            BootstrapServers = _fixture.KafkaBootstrapServers,
            GroupId          = $"test-consumer-{Guid.NewGuid()}",
            AutoOffsetReset  = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe("proposta-contratada");

        var mensagemRecebida = false;
        var deadline         = DateTime.UtcNow.AddSeconds(15);

        while (DateTime.UtcNow < deadline)
        {
            var result = consumer.Consume(TimeSpan.FromSeconds(2));
            if (result?.Message?.Value is null) continue;
            result.Message.Value.Should().Contain(propostaId.ToString());
            mensagemRecebida = true;
            break;
        }

        consumer.Close();
        mensagemRecebida.Should().BeTrue("o evento PropostaContratadaEvent deve ter sido publicado no Kafka");
    }

    [SkippableFact]
    public async Task HealthCheck_Live_DeveRetornarHealthy()
    {
        Skip.IfNot(_podeRodar, "Testes Kafka ignorados -- Docker indisponivel ou RUN_INTEGRATION_TESTS != true");

        var response = await _client.GetAsync("/health/live");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    private async Task InserirPropostaAprovadaAsync(Guid propostaId, string cpf)
    {
        var cfg = _fixture.Services.GetService(typeof(IConfiguration)) as IConfiguration
            ?? throw new InvalidOperationException("IConfiguration nao encontrado");

        using var conn = new Npgsql.NpgsqlConnection(cfg.GetConnectionString("DefaultConnection"));
        await conn.OpenAsync();

        const string sql = @"
            INSERT INTO proposta.propostas (id, cpf, tipo_seguro, valor_premio, status, criado_em)
            VALUES (@Id, @Cpf, 1, 100.00, 'Aprovada', NOW())
            ON CONFLICT (id) DO NOTHING";

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@Id",  propostaId);
        cmd.Parameters.AddWithValue("@Cpf", cpf);
        await cmd.ExecuteNonQueryAsync();
    }
}

