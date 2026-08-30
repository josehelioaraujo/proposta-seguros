using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PropostaService.IntegrationTests.Fixtures;
using Xunit;

namespace PropostaService.IntegrationTests;

public class PropostaInMemoryTests : IClassFixture<InMemoryApiFixture>
{
    private readonly HttpClient _client;
    private static readonly bool _integrationEnabled =
        Environment.GetEnvironmentVariable("RUN_INTEGRATION_TESTS") == "true";

    public PropostaInMemoryTests(InMemoryApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [SkippableFact]
    public async Task CriarProposta_InMemory_DeveRetornar201()
    {
        Skip.IfNot(_integrationEnabled, "Testes de integração desabilitados (RUN_INTEGRATION_TESTS != true)");

        var payload = new
        {
            nomeCliente = "João Silva",
            cpf         = "529.982.247-25",
            tipoSeguro  = 2,
            valor       = 100m
        };

        var response = await _client.PostAsJsonAsync("/api/Propostas", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [SkippableFact]
    public async Task ListarPropostas_InMemory_DeveRetornar200()
    {
        Skip.IfNot(_integrationEnabled, "Testes de integração desabilitados (RUN_INTEGRATION_TESTS != true)");

        var response = await _client.GetAsync("/api/Propostas");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [SkippableFact]
    public async Task HealthCheck_InMemory_DeveRetornarHealthy()
    {
        Skip.IfNot(_integrationEnabled, "Testes de integração desabilitados (RUN_INTEGRATION_TESTS != true)");

        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [SkippableFact]
    public async Task CriarProposta_InMemory_CpfInvalido_DeveRetornar400()
    {
        Skip.IfNot(_integrationEnabled, "Testes de integração desabilitados (RUN_INTEGRATION_TESTS != true)");

        var payload = new
        {
            nomeCliente = "João Silva",
            cpf         = "000.000.000-00",
            tipoSeguro  = 2,
            valor       = 100m
        };

        var response = await _client.PostAsJsonAsync("/api/Propostas", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
