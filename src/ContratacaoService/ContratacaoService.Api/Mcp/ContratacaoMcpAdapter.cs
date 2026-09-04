using ModelContextProtocol.Server;
using ContratacaoService.Application.UseCases.ContratarProposta;
using ContratacaoService.Application.UseCases.ObterContratacao;
using System.ComponentModel;
using System.Text.Json;

namespace ContratacaoService.Api.Mcp;

[McpServerToolType]
public class ContratacaoMcpAdapter
{
    private readonly ContratarPropostaUseCase _contratarProposta;
    private readonly ObterContratacaoUseCase  _obterContratacao;

    public ContratacaoMcpAdapter(
        ContratarPropostaUseCase contratarProposta,
        ObterContratacaoUseCase  obterContratacao)
    {
        _contratarProposta = contratarProposta;
        _obterContratacao  = obterContratacao;
    }

    [McpServerTool, Description("Contrata uma proposta de seguro aprovada. A proposta precisa estar com status Aprovada para ser contratada.")]
    public async Task<string> contratar_proposta(
        [Description("ID da proposta aprovada (GUID)")] Guid propostaId,
        [Description("CPF do cliente no formato 000.000.000-00")] string cpf)
    {
        var request = new ContratarPropostaRequest(propostaId, cpf);
        var result  = await _contratarProposta.ExecuteAsync(request);

        return result.Success
            ? JsonSerializer.Serialize(result.Data, JsonOpts)
            : $"Erro {result.Status}: {result.Error}";
    }

    [McpServerTool, Description("ObtÃ©m uma contrataÃ§Ã£o pelo ID.")]
    public async Task<string> obter_contratacao(
        [Description("ID da contrataÃ§Ã£o (GUID)")] Guid id)
    {
        var result = await _obterContratacao.ExecuteAsync(id);

        return result.Success
            ? JsonSerializer.Serialize(result.Data, JsonOpts)
            : $"Erro {result.Status}: {result.Error}";
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true
    };
}

