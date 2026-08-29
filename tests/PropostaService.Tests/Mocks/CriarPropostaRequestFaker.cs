using Bogus;
using Bogus.Extensions.Brazil;
using PropostaService.Application.UseCases.CriarProposta;
using PropostaService.Domain.Enums;

namespace PropostaService.Tests.Mocks;

public class CriarPropostaRequestFaker : Faker<CriarPropostaRequest>
{
    public CriarPropostaRequestFaker()
    {
        CustomInstantiator(f => new CriarPropostaRequest(
            NomeCliente: f.Name.FullName(),
            Cpf:         f.Person.Cpf(includeFormatSymbols: false),
            TipoSeguro:  f.PickRandom<TipoSeguro>(),
            Valor:       f.Random.Decimal(50m, 5000m)
        ));
    }
}
