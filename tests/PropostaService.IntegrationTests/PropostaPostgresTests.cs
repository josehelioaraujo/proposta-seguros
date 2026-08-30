using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PropostaService.IntegrationTests.Fixtures;
using Xunit;

namespace PropostaService.IntegrationTests;

public class PropostaPostgresTests : IClassFixture<PostgresApiFixture>
{
    private readonly HttpClient _client;
    private static readonly bool _integrationEnabled =
        Environment.GetEnvironmentVariable("RUN_INTEGRATION_TESTS") == "true";
    private static readonly bool _podeRodar =
        _integrationEnabled && PostgresApiFixture.DockerDisponivel;

    public PropostaPostgresTests(PostgresApiFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [SkippableFact]
    public async Task CriarProposta_Postgres_DeveRetornar201EPersistir()
    {
        Skip.IfNot(_podeRodar, "Testes PostgreSQL ignorados — Docker indisponível ou RUN_INTEGRATION_TESTS != true");

        var payload = new
        {
            nomeCliente = "Maria Souza",
            cpf         = "529.982.247-25",
            tipoSeguro  = 1,
            valor       = 100m
        };

        var createResponse = await _client.PostAsJsonAsync("/api/Propostas", payload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<dynamic>();
        string id = created!.GetProperty("id").GetString()!;

        var getResponse = await _client.GetAsync($"/api/Propostas/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [SkippableFact]
    public async Task HealthCheck_Postgres_DeveReportarBancoHealthy()
    {
        Skip.IfNot(_podeRodar, "Testes PostgreSQL ignorados — Docker indisponível ou RUN_INTEGRATION_TESTS != true");

        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Healthy");
    }

    [SkippableFact]
    public async Task CriarPropostaDuplicada_Postgres_DeveRetornar409()
    {
        Skip.IfNot(_podeRodar, "Testes PostgreSQL ignorados — Docker indisponível ou RUN_INTEGRATION_TESTS != true");

        var payload = new
        {
            nomeCliente = "Carlos Lima",
            cpf         = "346.859.588-37",
            tipoSeguro  = 3,
            valor       = 50m
        };

        await _client.PostAsJsonAsync("/api/Propostas", payload);
        var response = await _client.PostAsJsonAsync("/api/Propostas", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
