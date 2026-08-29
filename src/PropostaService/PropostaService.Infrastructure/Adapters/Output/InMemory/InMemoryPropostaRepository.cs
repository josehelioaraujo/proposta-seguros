using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using PropostaService.Domain.Ports;

namespace PropostaService.Infrastructure.Adapters.InMemory;

public class InMemoryPropostaRepository : IPropostaRepository
{
    private readonly List<Proposta> _propostas = [];

    public Task<Proposta?> GetByIdAsync(Guid id)
        => Task.FromResult(_propostas.FirstOrDefault(p => p.Id == id));

    public Task<IEnumerable<Proposta>> GetAllAsync()
        => Task.FromResult(_propostas.AsEnumerable());

    public Task AddAsync(Proposta proposta)
    {
        _propostas.Add(proposta);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Proposta proposta)
    {
        var index = _propostas.FindIndex(p => p.Id == proposta.Id);
        if (index >= 0) _propostas[index] = proposta;
        return Task.CompletedTask;
    }

    public Task<Proposta?> BuscarPorCpfETipoAsync(string cpf, TipoSeguro tipo)
        => Task.FromResult(
            _propostas.FirstOrDefault(p =>
                p.Cpf == cpf &&
                p.TipoSeguro == tipo &&
                p.Status == Domain.Enums.PropostaStatus.EmAnalise));
}
