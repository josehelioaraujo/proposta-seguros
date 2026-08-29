using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;

namespace PropostaService.Infrastructure.Adapters.Database;

internal record PropostaRow(
    Guid      Id,
    string    NomeCliente,
    string    Cpf,
    int       TipoSeguro,
    decimal   Valor,
    int       Status,
    DateTime  CriadoEm,
    DateTime? AtualizadoEm
)
{
    public Proposta ToDomain() => Proposta.Reconstituir(
        Id,
        NomeCliente,
        Cpf,
        (TipoSeguro)TipoSeguro,
        Valor,
        (PropostaStatus)Status,
        CriadoEm,
        AtualizadoEm);
}
