using PropostaService.Application.UseCases.CriarProposta;
using PropostaService.Domain.Ports;
using PropostaService.Domain.Shared;

namespace PropostaService.Application.UseCases.AlterarStatus;

public class AlterarStatusUseCase
{
    private readonly IPropostaRepository _repository;

    public AlterarStatusUseCase(IPropostaRepository repository)
        => _repository = repository;

    public async Task<Result<PropostaResponse>> ExecuteAsync(AlterarStatusRequest request)
    {
        var proposta = await _repository.GetByIdAsync(request.Id);
        if (proposta is null)
            return Result<PropostaResponse>.NotFound("Proposta nao encontrada.");

        // Regra encapsulada na entidade — status final e imutavel
        if (proposta.StatusFinal)
            return Result<PropostaResponse>.Unprocessable(
                $"Proposta com status {proposta.Status} nao pode ser alterada.");

        proposta.AlterarStatus(request.NovoStatus);
        await _repository.UpdateAsync(proposta);

        return Result<PropostaResponse>.Ok(PropostaResponse.FromEntity(proposta));
    }
}
