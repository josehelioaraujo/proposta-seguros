using Bogus;
using Bogus.Extensions.Brazil;
using ContratacaoService.Domain.Entities;

namespace ContratacaoService.Tests.Mocks;

public class ContratacaoFaker : Faker<Contratacao>
{
    public ContratacaoFaker()
    {
        CustomInstantiator(f => Contratacao.Criar(
            propostaId: Guid.NewGuid(),
            cpf:        f.Person.Cpf(includeFormatSymbols: false)
        ));
    }
}
