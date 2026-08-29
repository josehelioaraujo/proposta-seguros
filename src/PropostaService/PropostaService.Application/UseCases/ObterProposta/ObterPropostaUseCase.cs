using PropostaService.Application.UseCases.CriarProposta;
using PropostaService.Domain.Ports;
using PropostaService.Domain.Shared;

namespace PropostaService.Application.UseCases.ObterProposta;

public class ObterPropostaUseCase
{
    private readonly IPropostaRepository _repository;

    public ObterPropostaUseCase(IPropostaRepository repository)
        => _repository = repository;

    public async Task<Result<PropostaResponse>> ExecuteAsync(Guid id)
    {
        var proposta = await _repository.GetByIdAsync(id);
        if (proposta is null)
            return Result<PropostaResponse>.NotFound("Proposta nao encontrada.");

        return Result<PropostaResponse>.Ok(PropostaResponse.FromEntity(proposta));
    }
}
