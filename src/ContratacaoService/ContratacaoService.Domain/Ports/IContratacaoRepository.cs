using ContratacaoService.Domain.Entities;

namespace ContratacaoService.Domain.Ports;

public interface IContratacaoRepository
{
    Task<Contratacao?>  GetByIdAsync(Guid id);
    Task<Contratacao?>  GetByPropostaIdAsync(Guid propostaId);
    Task                AddAsync(Contratacao contratacao);
}
