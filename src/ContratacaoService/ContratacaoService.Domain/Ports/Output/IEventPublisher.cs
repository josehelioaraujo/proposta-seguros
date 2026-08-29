namespace ContratacaoService.Domain.Ports.Output;

public interface IEventPublisher
{
    Task PublishAsync<T>(string exchange, string routingKey, T message) where T : class;
}
