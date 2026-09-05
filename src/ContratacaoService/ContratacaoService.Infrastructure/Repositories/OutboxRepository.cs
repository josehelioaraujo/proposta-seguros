using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Ports.Output;
using Dapper;
using Npgsql;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace ContratacaoService.Infrastructure.Repositories;

public class OutboxRepository(IConfiguration configuration) : IOutboxRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")!;

    public async Task SalvarAsync(OutboxMessage mensagem, IDbTransaction transacao, CancellationToken ct = default)
    {
        const string sql = """
            INSERT INTO contratacao.outbox (id, tipo, payload, criado_em, processado)
            VALUES (@Id, @Tipo, @Payload::jsonb, @CriadoEm, false)
            """;

        await transacao.Connection!.ExecuteAsync(sql, mensagem, transacao);
    }

    public async Task<IEnumerable<OutboxMessage>> ObterPendentesAsync(int limite = 50, CancellationToken ct = default)
    {
        const string sql = """
            SELECT id, tipo, payload, criado_em, processado, processado_em
            FROM contratacao.outbox
            WHERE processado = false
            ORDER BY criado_em
            LIMIT @Limite
            FOR UPDATE SKIP LOCKED
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        return await conn.QueryAsync<OutboxMessage>(sql, new { Limite = limite });
    }

    public async Task MarcarProcessadoAsync(Guid id, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE contratacao.outbox
            SET processado = true, processado_em = NOW()
            WHERE id = @Id
            """;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.ExecuteAsync(sql, new { Id = id });
    }
}
