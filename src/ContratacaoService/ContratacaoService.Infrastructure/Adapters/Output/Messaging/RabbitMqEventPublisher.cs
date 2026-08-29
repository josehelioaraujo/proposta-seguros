using System.Text;
using System.Text.Json;
using ContratacaoService.Domain.Ports.Output;
using ContratacaoService.Infrastructure.Settings;
using RabbitMQ.Client;

namespace ContratacaoService.Infrastructure.Adapters.Output.Messaging;

public class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel    _channel;
    private readonly RabbitMqSettings _settings;

    private RabbitMqEventPublisher(
        IConnection       connection,
        IChannel          channel,
        RabbitMqSettings  settings)
    {
        _connection = connection;
        _channel    = channel;
        _settings   = settings;
    }

    // Factory async — RabbitMQ.Client 7.x exige async
    public static async Task<RabbitMqEventPublisher> CreateAsync(RabbitMqSettings settings)
    {
        var factory = new ConnectionFactory
        {
            HostName    = settings.Host,
            Port        = settings.Port,
            UserName    = settings.Username,
            Password    = settings.Password,
            VirtualHost = settings.VirtualHost
        };

        var connection = await factory.CreateConnectionAsync();
        var channel    = await connection.CreateChannelAsync();

        // Declara exchange e fila
        await channel.ExchangeDeclareAsync(
            exchange:    settings.Exchange,
            type:        ExchangeType.Direct,
            durable:     true,
            autoDelete:  false);

        await channel.QueueDeclareAsync(
            queue:      settings.Queue,
            durable:    true,
            exclusive:  false,
            autoDelete: false);

        await channel.QueueBindAsync(
            queue:      settings.Queue,
            exchange:   settings.Exchange,
            routingKey: settings.RoutingKey);

        return new RabbitMqEventPublisher(connection, channel, settings);
    }

    public async Task PublishAsync<T>(string exchange, string routingKey, T message)
        where T : class
    {
        var json  = JsonSerializer.Serialize(message);
        var body  = Encoding.UTF8.GetBytes(json);

        var props = new BasicProperties
        {
            Persistent   = true,
            ContentType  = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await _channel.BasicPublishAsync(
            exchange:   exchange,
            routingKey: routingKey,
            mandatory:  false,
            basicProperties: props,
            body:       body);
    }

    public async ValueTask DisposeAsync()
    {
        await _channel.CloseAsync();
        await _connection.CloseAsync();
    }
}
