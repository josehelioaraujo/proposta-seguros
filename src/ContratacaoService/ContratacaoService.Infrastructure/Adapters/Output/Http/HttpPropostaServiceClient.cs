using System.Net.Http.Json;
using ContratacaoService.Domain.Ports.Output;

namespace ContratacaoService.Infrastructure.Adapters.Output.Http;

public class HttpPropostaServiceClient : IPropostaServiceClient
{
    private readonly HttpClient _httpClient;

    public HttpPropostaServiceClient(HttpClient httpClient)
        => _httpClient = httpClient;

    public async Task<PropostaDto?> ObterPropostaAsync(Guid propostaId)
    {
        var response = await _httpClient.GetAsync($"/api/propostas/{propostaId}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<PropostaDto>();
    }
}
