namespace ContratacaoService.Domain.Events;

public record PropostaContratadaEvent(
    Guid     ContratacaoId,
    Guid     PropostaId,
    string   Cpf,
    DateTime DataContratacao,
    DateTime OcorridoEm
);
