using PropostaService.Domain.Entities;

namespace PropostaService.Application.UseCases.CriarProposta;

public record PropostaResponse(
    Guid     Id,
    string   NomeCliente,
    string   Cpf,
    string   TipoSeguro,
    decimal  Valor,
    string   Status,
    DateTime CriadoEm,
    DateTime? AtualizadoEm
)
{
    public static PropostaResponse FromEntity(Proposta proposta) => new(
        proposta.Id,
        proposta.NomeCliente,
        proposta.Cpf,
        proposta.TipoSeguro.ToString(),
        proposta.Valor,
        proposta.Status.ToString(),
        proposta.CriadoEm,
        proposta.AtualizadoEm
    );
}
