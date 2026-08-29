using ContratacaoService.Domain.Ports.Output;

namespace ContratacaoService.Infrastructure.Adapters.Output.Messaging;

/// <summary>
/// Publisher nulo — usado quando RabbitMQ esta indisponivel.
/// Garante que a API nao quebre mesmo sem mensageria.
/// </summary>
public class NullEventPublisher : IEventPublisher
{
    public Task PublishAsync<T>(string exchange, string routingKey, T message)
        where T : class
    {
        return Task.CompletedTask;
    }
}
