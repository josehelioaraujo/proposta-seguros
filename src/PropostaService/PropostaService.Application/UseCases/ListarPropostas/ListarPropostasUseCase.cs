using Microsoft.Extensions.Logging;
using PropostaService.Application.UseCases.CriarProposta;
using PropostaService.Domain.Ports.Output;
using PropostaService.Domain.Shared;

namespace PropostaService.Application.UseCases.ListarPropostas;

public class ListarPropostasUseCase
{
    private readonly IPropostaRepository              _repository;
    private readonly ILogger<ListarPropostasUseCase>  _logger;

    public ListarPropostasUseCase(
        IPropostaRepository             repository,
        ILogger<ListarPropostasUseCase> logger)
    {
        _repository = repository;
        _logger     = logger;
    }

    public async Task<Result<IEnumerable<PropostaResponse>>> ExecuteAsync()
    {
        _logger.LogInformation("Listando todas as propostas");

        var propostas = await _repository.GetAllAsync();
        var response  = propostas.Select(PropostaResponse.FromEntity);

        _logger.LogInformation("Total de propostas encontradas: {Total}", response.Count());

        return Result<IEnumerable<PropostaResponse>>.Ok(response);
    }
}
