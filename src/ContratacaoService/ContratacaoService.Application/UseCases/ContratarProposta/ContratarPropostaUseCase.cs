using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Ports;
using ContratacaoService.Domain.Shared;

namespace ContratacaoService.Application.UseCases.ContratarProposta;

public class ContratarPropostaUseCase
{
    private readonly IContratacaoRepository  _repository;
    private readonly IPropostaServiceClient  _propostaClient;

    public ContratarPropostaUseCase(
        IContratacaoRepository repository,
        IPropostaServiceClient propostaClient)
    {
        _repository     = repository;
        _propostaClient = propostaClient;
    }

    public async Task<Result<ContratacaoResponse>> ExecuteAsync(ContratarPropostaRequest request)
    {
        // Idempotencia — proposta ja contratada
        var existente = await _repository.GetByPropostaIdAsync(request.PropostaId);
        if (existente is not null)
            return Result<ContratacaoResponse>.Conflict(
                "Ja existe uma contratacao para esta proposta.");

        // Busca proposta no PropostaService
        var proposta = await _propostaClient.ObterPropostaAsync(request.PropostaId);
        if (proposta is null)
            return Result<ContratacaoResponse>.NotFound("Proposta nao encontrada.");

        // Valida status Aprovada
        if (proposta.Status != "Aprovada")
            return Result<ContratacaoResponse>.Unprocessable(
                $"Proposta com status '{proposta.Status}' nao pode ser contratada. Status necessario: Aprovada.");

        // Cria contratacao
        var contratacao = Contratacao.Criar(request.PropostaId, request.Cpf);
        await _repository.AddAsync(contratacao);

        return Result<ContratacaoResponse>.Created(ContratacaoResponse.FromEntity(contratacao));
    }
}
