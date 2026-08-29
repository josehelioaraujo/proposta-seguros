namespace ContratacaoService.Infrastructure.Settings;

public record RabbitMqSettings
{
    public string Host        { get; init; } = "localhost";
    public int    Port        { get; init; } = 5672;
    public string Username    { get; init; } = "guest";
    public string Password    { get; init; } = "guest";
    public string VirtualHost { get; init; } = "/";
    public string Exchange    { get; init; } = "proposta.exchange";
    public string Queue       { get; init; } = "proposta.contratada.queue";
    public string RoutingKey  { get; init; } = "proposta.contratada";
}
