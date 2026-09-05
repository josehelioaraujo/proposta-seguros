using System.Net.Http.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using FluentAssertions;
using ContratacaoService.IntegrationTests.Fixtures;
using Microsoft.Extensions.Configuration;
using Xunit;

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

        // Registra proposta aprovada no fake - sem chamada HTTP
        _fixture.FakePropostaClient.Registrar(propostaId, "Aprovada");

        // Criar topico antes de publicar
        var adminConfig = new AdminClientConfig { BootstrapServers = _fixture.KafkaBootstrapServers };
        using var adminClient = new AdminClientBuilder(adminConfig).Build();
        try
        {
            await adminClient.CreateTopicsAsync(new[]
            {
                new TopicSpecification
                {
                    Name = "proposta-contratada",
                    NumPartitions = 1,
                    ReplicationFactor = 1
                }
            });
        }
        catch (CreateTopicsException) { /* topico ja existe */ }

        // Contratar via API
        var payload  = new { propostaId, cpf };
        var response = await _client.PostAsJsonAsync("/api/Contratacoes", payload);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        // Aguardar OutboxRelayWorker publicar (intervalo 5s + margem)
        await Task.Delay(TimeSpan.FromSeconds(8));

        // Consumir do Kafka e verificar mensagem
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _fixture.KafkaBootstrapServers,
            GroupId          = $"test-consumer-{Guid.NewGuid()}",
            AutoOffsetReset  = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
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
}
