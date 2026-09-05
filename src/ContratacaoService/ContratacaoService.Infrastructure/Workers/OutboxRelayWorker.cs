using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Ports.Output;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ContratacaoService.Infrastructure.Workers;

public class OutboxRelayWorker(
    IOutboxRepository outboxRepository,
    IEventPublisher eventPublisher,
    IConfiguration configuration,
    ILogger<OutboxRelayWorker> logger) : BackgroundService
{
    private readonly TimeSpan _intervalo = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OutboxRelayWorker iniciado — intervalo {Intervalo}s", _intervalo.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessarPendentesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OutboxRelayWorker: erro ao processar mensagens pendentes");
            }

            await Task.Delay(_intervalo, stoppingToken);
        }

        logger.LogInformation("OutboxRelayWorker encerrado");
    }

    private async Task ProcessarPendentesAsync(CancellationToken ct)
    {
        var pendentes = (await outboxRepository.ObterPendentesAsync(50, ct)).ToList();

        if (pendentes.Count == 0) return;

        logger.LogInformation("OutboxRelayWorker: {Total} mensagem(ns) pendente(s)", pendentes.Count);

        var topico = configuration["Kafka:Topicos:PropostaContratadaEvent"]
                  ?? configuration["RabbitMQ:Exchange"]
                  ?? "proposta-contratada";

        foreach (var mensagem in pendentes)
        {
            try
            {
                await eventPublisher.PublishAsync(topico, mensagem.Tipo, mensagem);
                await outboxRepository.MarcarProcessadoAsync(mensagem.Id, ct);

                logger.LogInformation("OutboxRelayWorker: {Id} ({Tipo}) publicado", mensagem.Id, mensagem.Tipo);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OutboxRelayWorker: falha em {Id} ({Tipo}) — será reprocessado", mensagem.Id, mensagem.Tipo);
            }
        }
    }
}
