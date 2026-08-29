using Microsoft.Extensions.Logging;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Events;
using ContratacaoService.Domain.Ports.Output;
using ContratacaoService.Domain.Shared;

namespace ContratacaoService.Application.UseCases.ContratarProposta;

public class ContratarPropostaUseCase
{
    private readonly IContratacaoRepository              _repository;
    private readonly IPropostaServiceClient              _propostaClient;
    private readonly IEventPublisher?                    _eventPublisher;
    private readonly ILogger<ContratarPropostaUseCase>   _logger;

    public ContratarPropostaUseCase(
        IContratacaoRepository             repository,
        IPropostaServiceClient             propostaClient,
        ILogger<ContratarPropostaUseCase>  logger,
        IEventPublisher?                   eventPublisher = null)
    {
        _repository     = repository;
        _propostaClient = propostaClient;
        _logger         = logger;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<ContratacaoResponse>> ExecuteAsync(ContratarPropostaRequest request)
    {
        _logger.LogInformation(
            "Iniciando contratacao — PropostaID: {PropostaId} | CPF: {Cpf}",
            request.PropostaId, request.Cpf);

        // Idempotencia
        var existente = await _repository.GetByPropostaIdAsync(request.PropostaId);
        if (existente is not null)
        {
            _logger.LogWarning(
                "Contratacao duplicada — PropostaID: {PropostaId} | ContratacaoID: {Id}",
                request.PropostaId, existente.Id);

            return Result<ContratacaoResponse>.Conflict(
                "Ja existe uma contratacao para esta proposta.");
        }

        // Busca proposta
        _logger.LogInformation(
            "Consultando PropostaService — PropostaID: {PropostaId}",
            request.PropostaId);

        var proposta = await _propostaClient.ObterPropostaAsync(request.PropostaId);
        if (proposta is null)
        {
            _logger.LogWarning(
                "Proposta nao encontrada no PropostaService — ID: {PropostaId}",
                request.PropostaId);

            return Result<ContratacaoResponse>.NotFound("Proposta nao encontrada.");
        }

        // Valida status
        if (proposta.Status != "Aprovada")
        {
            _logger.LogWarning(
                "Proposta com status invalido para contratacao — ID: {PropostaId} | Status: {Status}",
                request.PropostaId, proposta.Status);

            return Result<ContratacaoResponse>.Unprocessable(
                $"Proposta com status '{proposta.Status}' nao pode ser contratada. Status necessario: Aprovada.");
        }

        // Cria contratacao
        var contratacao = Contratacao.Criar(request.PropostaId, request.Cpf);
        await _repository.AddAsync(contratacao);

        _logger.LogInformation(
            "Contratacao realizada com sucesso — ID: {Id} | PropostaID: {PropostaId}",
            contratacao.Id, contratacao.PropostaId);

        // Publica evento
        if (_eventPublisher is not null)
        {
            _logger.LogInformation(
                "Publicando evento PropostaContratadaEvent — ContratacaoID: {Id}",
                contratacao.Id);

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

            _logger.LogInformation(
                "Evento publicado com sucesso — ContratacaoID: {Id}",
                contratacao.Id);
        }

        return Result<ContratacaoResponse>.Created(ContratacaoResponse.FromEntity(contratacao));
    }
}
