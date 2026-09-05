using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Ports.Output;
using System.Data;

namespace ContratacaoService.Infrastructure.Adapters.Output.InMemory;

public class NullOutboxRepository : IOutboxRepository
{
    public Task SalvarAsync(OutboxMessage mensagem, IDbTransaction transacao, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<IEnumerable<OutboxMessage>> ObterPendentesAsync(int limite = 50, CancellationToken ct = default)
        => Task.FromResult(Enumerable.Empty<OutboxMessage>());

    public Task MarcarProcessadoAsync(Guid id, CancellationToken ct = default)
        => Task.CompletedTask;
}
