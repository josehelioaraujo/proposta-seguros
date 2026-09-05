namespace ContratacaoService.Domain.Events;

public record PropostaContratadaEvent(
    Guid ContratacaoId,
    Guid PropostaId,
    string CpfCliente,
    int TipoSeguro,
    DateTime ContratadoEm);
