using PropostaService.Domain.Enums;

namespace PropostaService.Application.UseCases.CriarProposta;

public record CriarPropostaRequest(
    string     NomeCliente,
    string     Cpf,
    TipoSeguro TipoSeguro,
    decimal    Valor
);
