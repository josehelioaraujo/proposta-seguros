using System.Data;
using Dapper;
using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Ports.Output;
using ContratacaoService.Infrastructure.Adapters.Output.Database.Queries;

namespace ContratacaoService.Infrastructure.Adapters.Output.Database;

public class DapperContratacaoRepository : IContratacaoRepository
{
    private readonly IDbConnection _connection;

    public DapperContratacaoRepository(IDbConnection connection)
        => _connection = connection;

    public async Task<Contratacao?> GetByIdAsync(Guid id)
    {
        var row = await _connection.QueryFirstOrDefaultAsync<ContratacaoRow>(
            ContratacaoQueries.GetById, new { Id = id });
        return row?.ToDomain();
    }

    public async Task<Contratacao?> GetByPropostaIdAsync(Guid propostaId)
    {
        var row = await _connection.QueryFirstOrDefaultAsync<ContratacaoRow>(
            ContratacaoQueries.GetByPropostaId, new { PropostaId = propostaId });
        return row?.ToDomain();
    }

    public async Task AddAsync(Contratacao contratacao)
        => await _connection.ExecuteAsync(ContratacaoQueries.Insert, new
        {
            contratacao.Id,
            contratacao.PropostaId,
            contratacao.Cpf,
            contratacao.DataContratacao,
            contratacao.CriadoEm
        });
}
