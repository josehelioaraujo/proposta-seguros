using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ContratacaoService.Domain.Ports;
using ContratacaoService.Infrastructure.Adapters.Database;
using ContratacaoService.Infrastructure.Adapters.Http;
using ContratacaoService.Infrastructure.Adapters.InMemory;

namespace ContratacaoService.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Feature Flag — InMemory ou Dapper
        var usarBancoDados = configuration.GetValue<bool>("Features:UsarBancoDados");

        if (usarBancoDados)
        {
            services.AddScoped<IDbConnection>(_ =>
                new NpgsqlConnection(
                    configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IContratacaoRepository, DapperContratacaoRepository>();
        }
        else
        {
            services.AddSingleton<IContratacaoRepository, InMemoryContratacaoRepository>();
        }

        // HttpClient para comunicacao com PropostaService
        services.AddHttpClient<IPropostaServiceClient, HttpPropostaServiceClient>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["Services:PropostaService"]
                ?? "http://localhost:5001");
        });

        return services;
    }
}
