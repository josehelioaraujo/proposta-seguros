using Microsoft.AspNetCore.Mvc;
using PropostaService.Application.UseCases.AlterarStatus;
using PropostaService.Application.UseCases.CriarProposta;
using PropostaService.Application.UseCases.ListarPropostas;
using PropostaService.Application.UseCases.ObterProposta;
using PropostaService.Api.Extensions;

namespace PropostaService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropostasController : ControllerBase
{
    private readonly CriarPropostaUseCase    _criarProposta;
    private readonly ListarPropostasUseCase  _listarPropostas;
    private readonly ObterPropostaUseCase    _obterProposta;
    private readonly AlterarStatusUseCase    _alterarStatus;

    public PropostasController(
        CriarPropostaUseCase   criarProposta,
        ListarPropostasUseCase listarPropostas,
        ObterPropostaUseCase   obterProposta,
        AlterarStatusUseCase   alterarStatus)
    {
        _criarProposta   = criarProposta;
        _listarPropostas = listarPropostas;
        _obterProposta   = obterProposta;
        _alterarStatus   = alterarStatus;
    }

    /// <summary>Cria uma nova proposta de seguro</summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarPropostaRequest request)
    {
        var result = await _criarProposta.ExecuteAsync(request);
        return result.ToActionResult();
    }

    /// <summary>Lista todas as propostas</summary>
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var result = await _listarPropostas.ExecuteAsync();
        return result.ToActionResult();
    }

    /// <summary>Obtem uma proposta pelo Id</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var result = await _obterProposta.ExecuteAsync(id);
        return result.ToActionResult();
    }

    /// <summary>Altera o status de uma proposta</summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> AlterarStatus(Guid id, [FromBody] AlterarStatusRequest request)
    {
        var requestComId = request with { Id = id };
        var result       = await _alterarStatus.ExecuteAsync(requestComId);
        return result.ToActionResult();
    }
}
