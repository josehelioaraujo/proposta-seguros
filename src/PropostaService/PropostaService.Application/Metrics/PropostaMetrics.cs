namespace PropostaService.Application.Metrics;

public static class PropostaMetrics
{
    public static readonly Prometheus.Counter PropostasCriadas = Prometheus.Metrics
        .CreateCounter("propostas_criadas_total", "Total de propostas criadas");

    public static readonly Prometheus.Counter PropostasAprovadas = Prometheus.Metrics
        .CreateCounter("propostas_aprovadas_total", "Total de propostas aprovadas");

    public static readonly Prometheus.Counter PropostasRejeitadas = Prometheus.Metrics
        .CreateCounter("propostas_rejeitadas_total", "Total de propostas rejeitadas");
}
