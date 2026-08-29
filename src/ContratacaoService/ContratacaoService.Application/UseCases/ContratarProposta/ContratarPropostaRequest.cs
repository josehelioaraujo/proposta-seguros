namespace ContratacaoService.Application.UseCases.ContratarProposta;

public record ContratarPropostaRequest(
    Guid   PropostaId,
    string Cpf
);
