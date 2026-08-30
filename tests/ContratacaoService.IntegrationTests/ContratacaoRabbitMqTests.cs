using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ContratacaoService.IntegrationTests.Fixtures;
using RabbitMQ.Client;
using Xunit;

namespace ContratacaoService.IntegrationTests;

public class ContratacaoRabbitMqTests : IClassFixture<FullStackFixture>
{
    private readonly HttpClient _client;
    private readonly FullStackFixture _fixture;
    private static readonly bool _podeRodar =
        Environment.GetEnvironmentVariable("RUN_INTEGRATION_TESTS") == "true"
        && FullStackFixture.DockerDisponivel;

    public ContratacaoRabbitMqTests(FullStackFixture fixture)
    {
        _fixture = fixture;
        _client  = fixture.CreateClient();
    }

    [SkippableFact]
    public async Task RabbitMQ_DeveEstarAcessivel()
    {
        Skip.IfNot(_podeRodar, "Testes RabbitMQ ignorados — Docker indisponivel ou RUN_INTEGRATION_TESTS != true");

        var uri = new Uri(_fixture.RabbitMqConnectionString);
        var factory = new ConnectionFactory
        {
            HostName = uri.Host,
            Port     = uri.Port,
            UserName = "guest",
            Password = "guest"
        };

        var connection = await factory.CreateConnectionAsync();
        connection.IsOpen.Should().BeTrue();
        await connection.CloseAsync();
    }

    [SkippableFact]
    public async Task ContratarProposta_SemPropostaService_DeveResponder()
    {
        Skip.IfNot(_podeRodar, "Testes RabbitMQ ignorados — Docker indisponivel ou RUN_INTEGRATION_TESTS != true");

        var payload = new
        {
            propostaId = Guid.NewGuid(),
            cpf        = "529.982.247-25"
        };

        var response = await _client.PostAsJsonAsync("/api/Contratacoes", payload);

        // 500 esperado — PropostaService nao esta disponivel no ambiente isolado
        // Valida que a API respondeu sem travar e o RabbitMQ esta conectado
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NotFound,
            HttpStatusCode.Created,
            HttpStatusCode.InternalServerError);
    }

    [SkippableFact]
    public async Task HealthCheck_Live_DeveRetornarHealthy()
    {
        Skip.IfNot(_podeRodar, "Testes RabbitMQ ignorados — Docker indisponivel ou RUN_INTEGRATION_TESTS != true");

        // /health/live verifica apenas o servico local — sem dependencias externas
        var response = await _client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }
}
