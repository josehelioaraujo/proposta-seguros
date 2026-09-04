namespace ContratacaoService.Infrastructure.Metrics;

public static class RabbitMqMetrics
{
    public static readonly Prometheus.Counter EventosPublicados = Prometheus.Metrics
        .CreateCounter("rabbitmq_eventos_publicados_total", "Total de eventos publicados no RabbitMQ");
}
