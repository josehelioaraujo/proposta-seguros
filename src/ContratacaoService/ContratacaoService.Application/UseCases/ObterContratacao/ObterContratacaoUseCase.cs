using ContratacaoService.Domain.Ports.Output;
using ContratacaoService.Domain.Shared;

namespace ContratacaoService.Application.UseCases.ObterContratacao;

public class ObterContratacaoUseCase
{
    private readonly IContratacaoRepository _repository;

    public ObterContratacaoUseCase(IContratacaoRepository repository)
        => _repository = repository;

    public async Task<Result<ContratacaoService.Application.UseCases.ContratarProposta.ContratacaoResponse>> ExecuteAsync(Guid id)
    {
        var contratacao = await _repository.GetByIdAsync(id);
        if (contratacao is null)
            return Result<ContratacaoService.Application.UseCases.ContratarProposta.ContratacaoResponse>.NotFound(
                "Contratacao nao encontrada.");

        return Result<ContratacaoService.Application.UseCases.ContratarProposta.ContratacaoResponse>.Ok(
            ContratacaoService.Application.UseCases.ContratarProposta.ContratacaoResponse.FromEntity(contratacao));
    }
}
