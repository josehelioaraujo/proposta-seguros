namespace ContratacaoService.Application.Metrics;

public static class ContratacaoMetrics
{
    public static readonly Prometheus.Counter ContracoesRealizadas = Prometheus.Metrics
        .CreateCounter("contratacoes_realizadas_total", "Total de contratacoes realizadas");
}
