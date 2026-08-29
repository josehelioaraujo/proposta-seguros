namespace ContratacaoService.Infrastructure.Adapters.Output.Database.Queries;

internal static class ContratacaoQueries
{
    internal const string GetById = """
        SELECT  id,
                proposta_id,
                cpf,
                data_contratacao,
                criado_em
        FROM    contratacao.contratacoes
        WHERE   id = @Id
        """;

    internal const string GetByPropostaId = """
        SELECT  id,
                proposta_id,
                cpf,
                data_contratacao,
                criado_em
        FROM    contratacao.contratacoes
        WHERE   proposta_id = @PropostaId
        """;

    internal const string Insert = """
        INSERT INTO contratacao.contratacoes
            (id, proposta_id, cpf, data_contratacao, criado_em)
        VALUES
            (@Id, @PropostaId, @Cpf, @DataContratacao, @CriadoEm)
        """;
}
