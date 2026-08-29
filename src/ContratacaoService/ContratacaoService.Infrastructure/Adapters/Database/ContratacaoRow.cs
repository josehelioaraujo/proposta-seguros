using ContratacaoService.Domain.Entities;

namespace ContratacaoService.Infrastructure.Adapters.Database;

internal record ContratacaoRow(
    Guid     Id,
    Guid     PropostaId,
    string   Cpf,
    DateTime DataContratacao,
    DateTime CriadoEm
)
{
    public Contratacao ToDomain() => Contratacao.Reconstituir(
        Id, PropostaId, Cpf, DataContratacao, CriadoEm);
}
