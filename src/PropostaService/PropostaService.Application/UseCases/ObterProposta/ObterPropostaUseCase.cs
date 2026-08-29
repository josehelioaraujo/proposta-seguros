using Microsoft.Extensions.Logging;
using PropostaService.Application.UseCases.CriarProposta;
using PropostaService.Domain.Ports.Output;
using PropostaService.Domain.Shared;

namespace PropostaService.Application.UseCases.ObterProposta;

public class ObterPropostaUseCase
{
    private readonly IPropostaRepository            _repository;
    private readonly ILogger<ObterPropostaUseCase>  _logger;

    public ObterPropostaUseCase(
        IPropostaRepository           repository,
        ILogger<ObterPropostaUseCase> logger)
    {
        _repository = repository;
        _logger     = logger;
    }

    public async Task<Result<PropostaResponse>> ExecuteAsync(Guid id)
    {
        _logger.LogInformation("Obtendo proposta â€” ID: {Id}", id);

        var proposta = await _repository.GetByIdAsync(id);
        if (proposta is null)
        {
            _logger.LogWarning("Proposta nao encontrada â€” ID: {Id}", id);
            return Result<PropostaResponse>.NotFound("Proposta nao encontrada.");
        }

        _logger.LogInformation(
            "Proposta encontrada â€” ID: {Id} | Status: {Status}",
            proposta.Id, proposta.Status);

        return Result<PropostaResponse>.Ok(PropostaResponse.FromEntity(proposta));
    }
}
