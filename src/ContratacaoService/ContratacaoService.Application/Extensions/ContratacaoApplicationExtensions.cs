using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ContratacaoService.Application.UseCases.ContratarProposta;
using ContratacaoService.Application.UseCases.ObterContratacao;

namespace ContratacaoService.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ContratarPropostaUseCase>();
        services.AddScoped<ObterContratacaoUseCase>();
        services.AddValidatorsFromAssemblyContaining<ContratarPropostaValidator>();
        return services;
    }
}
