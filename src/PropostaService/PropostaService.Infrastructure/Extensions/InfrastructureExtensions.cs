using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PropostaService.Domain.Ports.Output;
using PropostaService.Infrastructure.Adapters.Database;
using PropostaService.Infrastructure.Adapters.InMemory;
using PropostaService.Infrastructure.Strategies;

namespace PropostaService.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Strategy Pattern â€” registra todas as regras de seguro
        services.AddScoped<IRegraSeguro, SeguroFGTSProtegidoRegra>();
        services.AddScoped<IRegraSeguro, SeguroVidaFamiliarRegra>();
        services.AddScoped<IRegraSeguro, SeguroCartaoProtegidoRegra>();
        services.AddScoped<IRegraSeguro, SeguroProtecaoCreditoRegra>();
        services.AddScoped<IRegraSeguro, SeguroContaCelularRegra>();

        // Feature Flag â€” InMemory ou Dapper
        var usarBancoDados = configuration.GetValue<bool>("Features:UsarBancoDados");

        if (usarBancoDados)
        {
            services.AddScoped<IDbConnection>(_ =>
                new NpgsqlConnection(
                    configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IPropostaRepository, DapperPropostaRepository>();
        }
        else
        {
            services.AddSingleton<IPropostaRepository, InMemoryPropostaRepository>();
        }

        return services;
    }
}
