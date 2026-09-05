using ContratacaoService.Domain.Ports.Output;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ContratacaoService.Infrastructure.Workers;

public class OutboxPublisherWorker(
    IServiceScopeFactory scopeFactory,
    IEventPublisher eventPublisher,
    IConfiguration configuration,
    ILogger<OutboxPublisherWorker> logger) : BackgroundService
{
    private readonly TimeSpan _intervalo = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OutboxPublisherWorker iniciado — intervalo {Intervalo}s", _intervalo.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessarPendentesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OutboxPublisherWorker: erro ao processar mensagens pendentes");
            }

            await Task.Delay(_intervalo, stoppingToken);
        }

        logger.LogInformation("OutboxPublisherWorker encerrado");
    }

    private async Task ProcessarPendentesAsync(CancellationToken ct)
    {
        // Cria scope proprio para resolver IOutboxRepository (Scoped) a partir do Hosted Service (Singleton)
        await using var scope = scopeFactory.CreateAsyncScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

        var pendentes = (await outboxRepository.ObterPendentesAsync(50, ct)).ToList();

        if (pendentes.Count == 0) return;

        logger.LogInformation("OutboxPublisherWorker: {Total} mensagem(ns) pendente(s)", pendentes.Count);

        var topico = configuration["Kafka:Topicos:PropostaContratadaEvent"]
                  ?? configuration["RabbitMQ:Exchange"]
                  ?? "proposta-contratada";

        foreach (var mensagem in pendentes)
        {
            try
            {
                await eventPublisher.PublishAsync(topico, mensagem.Tipo, mensagem);
                await outboxRepository.MarcarProcessadoAsync(mensagem.Id, ct);

                logger.LogInformation("OutboxPublisherWorker: {Id} ({Tipo}) publicado", mensagem.Id, mensagem.Tipo);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OutboxPublisherWorker: falha em {Id} ({Tipo}) — sera reprocessado", mensagem.Id, mensagem.Tipo);
            }
        }
    }
}

