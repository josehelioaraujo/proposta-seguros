using PropostaService.Domain.Enums;

namespace PropostaService.Application.UseCases.AlterarStatus;

public record AlterarStatusBodyRequest(PropostaStatus NovoStatus);
