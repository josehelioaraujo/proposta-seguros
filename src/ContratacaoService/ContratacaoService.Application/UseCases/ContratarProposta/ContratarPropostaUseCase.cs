using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ContratacaoService.Application.Metrics;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Events;
using ContratacaoService.Domain.Ports.Output;
using ContratacaoService.Domain.Shared;
using Npgsql;

namespace ContratacaoService.Application.UseCases.ContratarProposta;

public class ContratarPropostaUseCase
{
    private readonly IContratacaoRepository             _repository;
    private readonly IOutboxRepository                  _outboxRepository;
    private readonly IPropostaServiceClient             _propostaClient;
    private readonly IEventPublisher?                   _eventPublisher;
    private readonly IConfiguration                     _configuration;
    private readonly ILogger<ContratarPropostaUseCase>  _logger;

    public ContratarPropostaUseCase(
        IContratacaoRepository            repository,
        IOutboxRepository                 outboxRepository,
        IPropostaServiceClient            propostaClient,
        IConfiguration                    configuration,
        ILogger<ContratarPropostaUseCase> logger,
        IEventPublisher?                  eventPublisher = null)
    {
        _repository       = repository;
        _outboxRepository = outboxRepository;
        _propostaClient   = propostaClient;
        _configuration    = configuration;
        _logger           = logger;
        _eventPublisher   = eventPublisher;
    }

    public async Task<Result<ContratacaoResponse>> ExecuteAsync(ContratarPropostaRequest request)
    {
        _logger.LogInformation(
            "Iniciando contratacao  PropostaID: {PropostaId} | CPF: {Cpf}",
            request.PropostaId, request.Cpf);

        var existente = await _repository.GetByPropostaIdAsync(request.PropostaId);
        if (existente is not null)
        {
            _logger.LogWarning(
                "Contratacao duplicada  PropostaID: {PropostaId} | ContratacaoID: {Id}",
                request.PropostaId, existente.Id);
            return Result<ContratacaoResponse>.Conflict("Ja existe uma contratacao para esta proposta.");
        }

        var proposta = await _propostaClient.ObterPropostaAsync(request.PropostaId);
        if (proposta is null)
        {
            _logger.LogWarning("Proposta nao encontrada  ID: {PropostaId}", request.PropostaId);
            return Result<ContratacaoResponse>.NotFound("Proposta nao encontrada.");
        }

        if (proposta.Status != "Aprovada")
        {
            _logger.LogWarning(
                "Proposta com status invalido  ID: {PropostaId} | Status: {Status}",
                request.PropostaId, proposta.Status);
            return Result<ContratacaoResponse>.Unprocessable(
                $"Proposta com status '{proposta.Status}' nao pode ser contratada. Status necessario: Aprovada.");
        }

        var contratacao = Contratacao.Criar(request.PropostaId, request.Cpf);

        var usarBanco  = _configuration.GetValue<bool>("Features:UsarBancoDados");
        var usarBroker = _eventPublisher is not null &&
                         (_configuration.GetValue<bool>("Features:UsarKafka") ||
                          _configuration.GetValue<bool>("Features:UsarRabbitMQ"));

        if (usarBanco && usarBroker)
        {
            var connStr = _configuration.GetConnectionString("DefaultConnection")!;
            await using var conn = new NpgsqlConnection(connStr);
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();

            try
            {
                await _repository.AddAsync(contratacao, tx);

                var evento = new PropostaContratadaEvent(
                    ContratacaoId:   contratacao.Id,
                    PropostaId:      contratacao.PropostaId,
                    Cpf:             contratacao.Cpf,
                    DataContratacao: contratacao.DataContratacao,
                    OcorridoEm:      DateTime.UtcNow);

                var outbox = OutboxMessage.Criar(evento);
                await _outboxRepository.SalvarAsync(outbox, tx);

                await tx.CommitAsync();

                _logger.LogInformation(
                    "Contratacao + Outbox gravados  ID: {Id} | OutboxID: {OutboxId}",
                    contratacao.Id, outbox.Id);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Falha ao gravar contratacao  rollback executado");
                return Result<ContratacaoResponse>.Fail("Erro ao processar contratacao.");
            }
        }
        else
        {
            await _repository.AddAsync(contratacao);

            if (_eventPublisher is not null)
            {
                var evento = new PropostaContratadaEvent(
                    ContratacaoId:   contratacao.Id,
                    PropostaId:      contratacao.PropostaId,
                    Cpf:             contratacao.Cpf,
                    DataContratacao: contratacao.DataContratacao,
                    OcorridoEm:      DateTime.UtcNow);

                await _eventPublisher.PublishAsync(
                    exchange:   "proposta.exchange",
                    routingKey: "proposta.contratada",
                    message:    evento);

                _logger.LogInformation("Evento publicado diretamente  ContratacaoID: {Id}", contratacao.Id);
            }
        }

        ContratacaoMetrics.ContracoesRealizadas.Inc();
        _logger.LogInformation(
            "Contratacao realizada  ID: {Id} | PropostaID: {PropostaId}",
            contratacao.Id, contratacao.PropostaId);

        return Result<ContratacaoResponse>.Created(ContratacaoResponse.FromEntity(contratacao));
    }
}

