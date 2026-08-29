using Microsoft.AspNetCore.Mvc;
using ContratacaoService.Application.UseCases.ContratarProposta;
using ContratacaoService.Application.UseCases.ObterContratacao;
using ContratacaoService.Api.Extensions;

namespace ContratacaoService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContratacoesController : ControllerBase
{
    private readonly ContratarPropostaUseCase _contratarProposta;
    private readonly ObterContratacaoUseCase  _obterContratacao;

    public ContratacoesController(
        ContratarPropostaUseCase contratarProposta,
        ObterContratacaoUseCase  obterContratacao)
    {
        _contratarProposta = contratarProposta;
        _obterContratacao  = obterContratacao;
    }

    /// <summary>Contrata uma proposta aprovada</summary>
    [HttpPost]
    public async Task<IActionResult> Contratar([FromBody] ContratarPropostaRequest request)
    {
        var result = await _contratarProposta.ExecuteAsync(request);
        return result.ToActionResult();
    }

    /// <summary>Obtem uma contratacao pelo Id</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var result = await _obterContratacao.ExecuteAsync(id);
        return result.ToActionResult();
    }
}
