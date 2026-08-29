namespace ContratacaoService.Domain.Ports;

public interface IPropostaServiceClient
{
    Task<PropostaDto?> ObterPropostaAsync(Guid propostaId);
}

public record PropostaDto(
    Guid   Id,
    string Status
);
