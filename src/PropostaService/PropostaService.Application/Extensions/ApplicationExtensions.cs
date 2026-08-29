using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PropostaService.Application.UseCases.AlterarStatus;
using PropostaService.Application.UseCases.CriarProposta;
using PropostaService.Application.UseCases.ListarPropostas;
using PropostaService.Application.UseCases.ObterProposta;

namespace PropostaService.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Use Cases
        services.AddScoped<CriarPropostaUseCase>();
        services.AddScoped<ListarPropostasUseCase>();
        services.AddScoped<AlterarStatusUseCase>();
        services.AddScoped<ObterPropostaUseCase>();

        // Validators
        services.AddValidatorsFromAssemblyContaining<CriarPropostaValidator>();

        return services;
    }
}
