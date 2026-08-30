using System.Text;
using System.Text.Json;
using ContratacaoService.Domain.Ports.Output;
using ContratacaoService.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace ContratacaoService.Infrastructure.Adapters.Output.Messaging;

public class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
{
    private readonly RabbitMqSettings                    _settings;
    private readonly ILogger<RabbitMqEventPublisher>     _logger;
    private IConnection? _connection;
    private IChannel?    _channel;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public RabbitMqEventPublisher(
        RabbitMqSettings                 settings,
        ILogger<RabbitMqEventPublisher>  logger)
    {
        _settings = settings;
        _logger   = logger;
    }

    private async Task EnsureConnectedAsync()
    {
        if (_channel is not null && _channel.IsOpen) return;

        await _lock.WaitAsync();
        try
        {
            if (_channel is not null && _channel.IsOpen) return;

            _logger.LogInformation("Conectando ao RabbitMQ em {Host}:{Port}...", _settings.Host, _settings.Port);

            var factory = new ConnectionFactory
            {
                HostName    = _settings.Host,
                Port        = _settings.Port,
                UserName    = _settings.Username,
                Password    = _settings.Password,
                VirtualHost = _settings.VirtualHost
            };

            _connection = await factory.CreateConnectionAsync();
            _channel    = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(
                exchange:   _settings.Exchange,
                type:       ExchangeType.Direct,
                durable:    true,
                autoDelete: false);

            await _channel.QueueDeclareAsync(
                queue:      _settings.Queue,
                durable:    true,
                exclusive:  false,
                autoDelete: false);

            await _channel.QueueBindAsync(
                queue:      _settings.Queue,
                exchange:   _settings.Exchange,
                routingKey: _settings.RoutingKey);

            _logger.LogInformation("RabbitMQ conectado com sucesso!");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task PublishAsync<T>(string exchange, string routingKey, T message)
        where T : class
    {
        await EnsureConnectedAsync();

        var json  = JsonSerializer.Serialize(message);
        var body  = Encoding.UTF8.GetBytes(json);

        var props = new BasicProperties
        {
            Persistent   = true,
            ContentType  = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await _channel!.BasicPublishAsync(
            exchange:        exchange,
            routingKey:      routingKey,
            mandatory:       false,
            basicProperties: props,
            body:            body);

        _logger.LogInformation("Evento publicado  Exchange: {Exchange} | RoutingKey: {RoutingKey}", exchange, routingKey);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null) await _channel.CloseAsync();
        if (_connection is not null) await _connection.CloseAsync();
    }
}
