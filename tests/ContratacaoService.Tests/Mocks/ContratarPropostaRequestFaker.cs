using Bogus;
using Bogus.Extensions.Brazil;
using ContratacaoService.Application.UseCases.ContratarProposta;

namespace ContratacaoService.Tests.Mocks;

public class ContratarPropostaRequestFaker : Faker<ContratarPropostaRequest>
{
    public ContratarPropostaRequestFaker()
    {
        CustomInstantiator(f => new ContratarPropostaRequest(
            PropostaId: Guid.NewGuid(),
            Cpf:        f.Person.Cpf(includeFormatSymbols: false)
        ));
    }
}
