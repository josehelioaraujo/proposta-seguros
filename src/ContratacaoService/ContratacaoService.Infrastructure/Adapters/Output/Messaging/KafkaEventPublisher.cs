using Confluent.Kafka;
using ContratacaoService.Domain.Ports.Output;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ContratacaoService.Infrastructure.Messaging;

public class KafkaEventPublisher(
    IConfiguration configuration,
    ILogger<KafkaEventPublisher> logger) : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer = new ProducerBuilder<string, string>(
        new ProducerConfig
        {
            BootstrapServers  = configuration["Kafka:BootstrapServers"] ?? "kafka:9092",
            Acks              = Acks.All,
            EnableIdempotence = true,
            MessageSendMaxRetries = 3
        }).Build();

    // exchange  tópico Kafka | routingKey  key da mensagem
    public async Task PublishAsync<T>(string exchange, string routingKey, T message)
        where T : class
    {
        var payload = JsonSerializer.Serialize(message);

        var kafkaMessage = new Message<string, string>
        {
            Key   = routingKey,
            Value = payload
        };

        try
        {
            var result = await _producer.ProduceAsync(exchange, kafkaMessage);
            logger.LogInformation(
                "Kafka: evento publicado  Tópico: {Topico} | Key: {Key} | Offset: {Offset}",
                exchange, routingKey, result.Offset);
        }
        catch (ProduceException<string, string> ex)
        {
            logger.LogError(ex,
                "Kafka: falha ao publicar  Tópico: {Topico} | Key: {Key}",
                exchange, routingKey);
            throw;
        }
    }

    public void Dispose() => _producer?.Dispose();
}

