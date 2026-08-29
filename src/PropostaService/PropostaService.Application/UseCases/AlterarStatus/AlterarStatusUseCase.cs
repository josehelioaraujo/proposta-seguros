using Microsoft.Extensions.Logging;
using PropostaService.Application.UseCases.CriarProposta;
using PropostaService.Domain.Ports.Output;
using PropostaService.Domain.Shared;

namespace PropostaService.Application.UseCases.AlterarStatus;

public class AlterarStatusUseCase
{
    private readonly IPropostaRepository            _repository;
    private readonly ILogger<AlterarStatusUseCase>  _logger;

    public AlterarStatusUseCase(
        IPropostaRepository           repository,
        ILogger<AlterarStatusUseCase> logger)
    {
        _repository = repository;
        _logger     = logger;
    }

    public async Task<Result<PropostaResponse>> ExecuteAsync(AlterarStatusRequest request)
    {
        _logger.LogInformation(
            "Alterando status â€” ID: {Id} | Novo status: {Status}",
            request.Id, request.NovoStatus);

        var proposta = await _repository.GetByIdAsync(request.Id);
        if (proposta is null)
        {
            _logger.LogWarning("Proposta nao encontrada â€” ID: {Id}", request.Id);
            return Result<PropostaResponse>.NotFound("Proposta nao encontrada.");
        }

        if (proposta.StatusFinal)
        {
            _logger.LogWarning(
                "Tentativa de alterar status final â€” ID: {Id} | Status atual: {Status}",
                proposta.Id, proposta.Status);

            return Result<PropostaResponse>.Unprocessable(
                $"Proposta com status {proposta.Status} nao pode ser alterada.");
        }

        proposta.AlterarStatus(request.NovoStatus);
        await _repository.UpdateAsync(proposta);

        _logger.LogInformation(
            "Status alterado com sucesso â€” ID: {Id} | Status: {Status}",
            proposta.Id, proposta.Status);

        return Result<PropostaResponse>.Ok(PropostaResponse.FromEntity(proposta));
    }
}
