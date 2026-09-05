using ContratacaoService.Domain.Ports.Output;

namespace ContratacaoService.IntegrationTests.Fakes;

/// <summary>
/// Retorna proposta aprovada diretamente sem chamar HTTP — usado nos testes Kafka
/// </summary>
public class FakePropostaServiceClient : IPropostaServiceClient
{
    private readonly Dictionary<Guid, PropostaDto> _propostas = new();

    public void Registrar(Guid id, string status = "Aprovada")
        => _propostas[id] = new PropostaDto(id, status);

    public Task<PropostaDto?> ObterPropostaAsync(Guid propostaId)
        => Task.FromResult(_propostas.TryGetValue(propostaId, out var p) ? p : null);
}
