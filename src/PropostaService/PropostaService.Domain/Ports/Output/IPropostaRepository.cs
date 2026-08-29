using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;

namespace PropostaService.Domain.Ports;

public interface IPropostaRepository
{
    Task<Proposta?>              GetByIdAsync(Guid id);
    Task<IEnumerable<Proposta>>  GetAllAsync();
    Task                         AddAsync(Proposta proposta);
    Task                         UpdateAsync(Proposta proposta);
    Task<Proposta?>              BuscarPorCpfETipoAsync(string cpf, TipoSeguro tipo);
}
