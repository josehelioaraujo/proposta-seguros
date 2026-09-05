using ContratacaoService.Domain.Entities;
using System.Data;

namespace ContratacaoService.Domain.Ports.Output;

public interface IOutboxRepository
{
    Task SalvarAsync(OutboxMessage mensagem, IDbTransaction transacao, CancellationToken ct = default);
    Task<IEnumerable<OutboxMessage>> ObterPendentesAsync(int limite = 50, CancellationToken ct = default);
    Task MarcarProcessadoAsync(Guid id, CancellationToken ct = default);
}
