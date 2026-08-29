using System.Data;
using Dapper;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Ports;
using PropostaService.Infrastructure.Adapters.Database.Queries;

namespace PropostaService.Infrastructure.Adapters.Database;

public class DapperPropostaRepository : IPropostaRepository
{
    private readonly IDbConnection _connection;

    public DapperPropostaRepository(IDbConnection connection)
        => _connection = connection;

    public async Task<Proposta?> GetByIdAsync(Guid id)
    {
        var row = await _connection.QueryFirstOrDefaultAsync<PropostaRow>(
            PropostaQueries.GetById, new { Id = id });

        return row?.ToDomain();
    }

    public async Task<IEnumerable<Proposta>> GetAllAsync()
    {
        var rows = await _connection.QueryAsync<PropostaRow>(PropostaQueries.GetAll);
        return rows.Select(r => r.ToDomain());
    }

    public async Task AddAsync(Proposta proposta)
        => await _connection.ExecuteAsync(PropostaQueries.Insert, new
        {
            proposta.Id,
            proposta.NomeCliente,
            proposta.Cpf,
            TipoSeguro = (int)proposta.TipoSeguro,
            proposta.Valor,
            Status     = (int)proposta.Status,
            proposta.CriadoEm
        });

    public async Task UpdateAsync(Proposta proposta)
        => await _connection.ExecuteAsync(PropostaQueries.UpdateStatus, new
        {
            Status       = (int)proposta.Status,
            AtualizadoEm = DateTime.UtcNow,
            proposta.Id
        });

    public async Task<Proposta?> BuscarPorCpfETipoAsync(string cpf, TipoSeguro tipo)
    {
        var row = await _connection.QueryFirstOrDefaultAsync<PropostaRow>(
            PropostaQueries.GetByCpfETipo, new
            {
                Cpf        = cpf,
                TipoSeguro = (int)tipo
            });

        return row?.ToDomain();
    }
}
