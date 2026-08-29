using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Events;
using ContratacaoService.Domain.Ports.Output;
using ContratacaoService.Domain.Shared;

namespace ContratacaoService.Application.UseCases.ContratarProposta;

public class ContratarPropostaUseCase
{
    private readonly IContratacaoRepository _repository;
    private readonly IPropostaServiceClient _propostaClient;
    private readonly IEventPublisher?       _eventPublisher;

    public ContratarPropostaUseCase(
        IContratacaoRepository repository,
        IPropostaServiceClient propostaClient,
        IEventPublisher?       eventPublisher = null)
    {
        _repository     = repository;
        _propostaClient = propostaClient;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<ContratacaoResponse>> ExecuteAsync(ContratarPropostaRequest request)
    {
        var existente = await _repository.GetByPropostaIdAsync(request.PropostaId);
        if (existente is not null)
            return Result<ContratacaoResponse>.Conflict(
                "Ja existe uma contratacao para esta proposta.");

        var proposta = await _propostaClient.ObterPropostaAsync(request.PropostaId);
        if (proposta is null)
            return Result<ContratacaoResponse>.NotFound("Proposta nao encontrada.");

        if (proposta.Status != "Aprovada")
            return Result<ContratacaoResponse>.Unprocessable(
                $"Proposta com status '{proposta.Status}' nao pode ser contratada. Status necessario: Aprovada.");

        var contratacao = Contratacao.Criar(request.PropostaId, request.Cpf);
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
        }

        return Result<ContratacaoResponse>.Created(ContratacaoResponse.FromEntity(contratacao));
    }
}
