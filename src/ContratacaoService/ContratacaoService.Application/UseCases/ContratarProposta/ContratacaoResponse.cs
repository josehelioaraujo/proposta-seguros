using ContratacaoService.Domain.Entities;

namespace ContratacaoService.Application.UseCases.ContratarProposta;

public record ContratacaoResponse(
    Guid     Id,
    Guid     PropostaId,
    string   Cpf,
    DateTime DataContratacao,
    DateTime CriadoEm
)
{
    public static ContratacaoResponse FromEntity(Contratacao contratacao) => new(
        contratacao.Id,
        contratacao.PropostaId,
        contratacao.Cpf,
        contratacao.DataContratacao,
        contratacao.CriadoEm
    );
}
