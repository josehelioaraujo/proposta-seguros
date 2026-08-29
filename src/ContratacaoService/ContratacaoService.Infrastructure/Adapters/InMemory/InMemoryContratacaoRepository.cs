using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Ports;

namespace ContratacaoService.Infrastructure.Adapters.InMemory;

public class InMemoryContratacaoRepository : IContratacaoRepository
{
    private readonly List<Contratacao> _contratacoes = [];

    public Task<Contratacao?> GetByIdAsync(Guid id)
        => Task.FromResult(_contratacoes.FirstOrDefault(c => c.Id == id));

    public Task<Contratacao?> GetByPropostaIdAsync(Guid propostaId)
        => Task.FromResult(_contratacoes.FirstOrDefault(c => c.PropostaId == propostaId));

    public Task AddAsync(Contratacao contratacao)
    {
        _contratacoes.Add(contratacao);
        return Task.CompletedTask;
    }
}
