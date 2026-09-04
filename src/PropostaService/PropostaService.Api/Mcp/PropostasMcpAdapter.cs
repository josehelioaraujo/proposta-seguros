using ModelContextProtocol.Server;
using PropostaService.Application.UseCases.AlterarStatus;
using PropostaService.Application.UseCases.CriarProposta;
using PropostaService.Application.UseCases.ListarPropostas;
using PropostaService.Application.UseCases.ObterProposta;
using PropostaService.Domain.Enums;
using System.ComponentModel;
using System.Text.Json;

namespace PropostaService.Api.Mcp;

[McpServerToolType]
public class PropostasMcpAdapter
{
    private readonly CriarPropostaUseCase   _criarProposta;
    private readonly ListarPropostasUseCase _listarPropostas;
    private readonly ObterPropostaUseCase   _obterProposta;
    private readonly AlterarStatusUseCase   _alterarStatus;

    public PropostasMcpAdapter(
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

    [McpServerTool, Description("Cria uma nova proposta de seguro. TipoSeguro: 1=SeguroFGTSProtegido, 2=SeguroVidaFamiliar, 3=SeguroCartaoProtegido, 4=ProtecaoCreditoTrabalhador, 5=SeguroContaCelularProtegidos.")]
    public async Task<string> criar_proposta(
        [Description("Nome completo do cliente")] string nomeCliente,
        [Description("CPF do cliente no formato 000.000.000-00")] string cpf,
        [Description("Tipo de seguro: 1 a 5")] int tipoSeguro,
        [Description("Valor da proposta em reais")] decimal valor)
    {
        var request = new CriarPropostaRequest(
            nomeCliente,
            cpf,
            (TipoSeguro)tipoSeguro,
            valor);

        var result = await _criarProposta.ExecuteAsync(request);

        return result.Success
            ? JsonSerializer.Serialize(result.Data, JsonOpts)
            : $"Erro {result.Status}: {result.Error}";
    }

    [McpServerTool, Description("Lista todas as propostas de seguro cadastradas.")]
    public async Task<string> listar_propostas()
    {
        var result = await _listarPropostas.ExecuteAsync();

        return result.Success
            ? JsonSerializer.Serialize(result.Data, JsonOpts)
            : $"Erro {result.Status}: {result.Error}";
    }

    [McpServerTool, Description("ObtÃ©m uma proposta de seguro pelo ID.")]
    public async Task<string> obter_proposta(
        [Description("ID da proposta (GUID)")] Guid id)
    {
        var result = await _obterProposta.ExecuteAsync(id);

        return result.Success
            ? JsonSerializer.Serialize(result.Data, JsonOpts)
            : $"Erro {result.Status}: {result.Error}";
    }

    [McpServerTool, Description("Altera o status de uma proposta. NovoStatus: 1=EmAnalise, 2=Aprovada, 3=Rejeitada. Status finais (Aprovada/Rejeitada) nao podem ser alterados.")]
    public async Task<string> alterar_status_proposta(
        [Description("ID da proposta (GUID)")] Guid id,
        [Description("Novo status: 1=EmAnalise, 2=Aprovada, 3=Rejeitada")] int novoStatus)
    {
        var request = new AlterarStatusRequest(id, (PropostaStatus)novoStatus);
        var result  = await _alterarStatus.ExecuteAsync(request);

        return result.Success
            ? JsonSerializer.Serialize(result.Data, JsonOpts)
            : $"Erro {result.Status}: {result.Error}";
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true
    };
}

