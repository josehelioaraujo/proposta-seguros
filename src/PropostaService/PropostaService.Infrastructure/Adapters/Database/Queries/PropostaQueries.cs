namespace PropostaService.Infrastructure.Adapters.Database.Queries;

internal static class PropostaQueries
{
    internal const string GetById = """
        SELECT  id,
                nome_cliente,
                cpf,
                tipo_seguro,
                valor,
                status,
                criado_em,
                atualizado_em
        FROM    proposta.propostas
        WHERE   id = @Id
        """;

    internal const string GetAll = """
        SELECT  id,
                nome_cliente,
                cpf,
                tipo_seguro,
                valor,
                status,
                criado_em,
                atualizado_em
        FROM    proposta.propostas
        ORDER BY criado_em DESC
        """;

    internal const string Insert = """
        INSERT INTO proposta.propostas
            (id, nome_cliente, cpf, tipo_seguro, valor, status, criado_em)
        VALUES
            (@Id, @NomeCliente, @Cpf, @TipoSeguro, @Valor, @Status, @CriadoEm)
        """;

    internal const string UpdateStatus = """
        UPDATE  proposta.propostas
        SET     status        = @Status,
                atualizado_em = @AtualizadoEm
        WHERE   id = @Id
        """;

    internal const string GetByCpfETipo = """
        SELECT  id,
                nome_cliente,
                cpf,
                tipo_seguro,
                valor,
                status,
                criado_em,
                atualizado_em
        FROM    proposta.propostas
        WHERE   cpf         = @Cpf
          AND   tipo_seguro = @TipoSeguro
          AND   status      = 1
        """;
}
