namespace ContratacaoService.Domain.Ports.Output;

public interface IPropostaServiceClient
{
    Task<PropostaDto?> ObterPropostaAsync(Guid propostaId);
}

public record PropostaDto(
    Guid   Id,
    string Status
);
