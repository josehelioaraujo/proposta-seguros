using PropostaService.Application.UseCases.CriarProposta;
using PropostaService.Domain.Ports;
using PropostaService.Domain.Shared;

namespace PropostaService.Application.UseCases.ListarPropostas;

public class ListarPropostasUseCase
{
    private readonly IPropostaRepository _repository;

    public ListarPropostasUseCase(IPropostaRepository repository)
        => _repository = repository;

    public async Task<Result<IEnumerable<PropostaResponse>>> ExecuteAsync()
    {
        var propostas = await _repository.GetAllAsync();
        var response  = propostas.Select(PropostaResponse.FromEntity);
        return Result<IEnumerable<PropostaResponse>>.Ok(response);
    }
}
