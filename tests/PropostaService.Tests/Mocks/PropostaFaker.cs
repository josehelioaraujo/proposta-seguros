using Bogus;
using Bogus.Extensions.Brazil;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;

namespace PropostaService.Tests.Mocks;

public class PropostaFaker : Faker<Proposta>
{
    public PropostaFaker()
    {
        CustomInstantiator(f => Proposta.Criar(
            nomeCliente: f.Name.FullName(),
            cpf:         f.Person.Cpf(includeFormatSymbols: false),
            tipoSeguro:  f.PickRandom<TipoSeguro>(),
            valor:       f.Random.Decimal(50m, 5000m)
        ));
    }

    public Proposta ComStatus(PropostaStatus status)
    {
        var proposta = Generate();
        if (status != PropostaStatus.EmAnalise)
            proposta.AlterarStatus(status);
        return proposta;
    }
}
