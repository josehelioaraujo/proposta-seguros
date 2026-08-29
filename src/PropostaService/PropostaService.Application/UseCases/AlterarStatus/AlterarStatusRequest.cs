using PropostaService.Domain.Enums;

namespace PropostaService.Application.UseCases.AlterarStatus;

public record AlterarStatusRequest(
    Guid          Id,
    PropostaStatus NovoStatus
);
