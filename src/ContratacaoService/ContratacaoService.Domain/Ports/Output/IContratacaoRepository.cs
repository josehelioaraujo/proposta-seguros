using ContratacaoService.Domain.Entities;
using System.Data;

namespace ContratacaoService.Domain.Ports.Output;

public interface IContratacaoRepository
{
    Task<Contratacao?>  GetByIdAsync(Guid id);
    Task<Contratacao?>  GetByPropostaIdAsync(Guid propostaId);
    Task                AddAsync(Contratacao contratacao);
    Task                AddAsync(Contratacao contratacao, IDbTransaction transacao);
}
