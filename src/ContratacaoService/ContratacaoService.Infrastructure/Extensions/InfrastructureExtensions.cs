using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using ContratacaoService.Domain.Ports.Output;
using ContratacaoService.Infrastructure.Adapters.Output.Database;
using ContratacaoService.Infrastructure.Adapters.Output.Http;
using ContratacaoService.Infrastructure.Adapters.Output.InMemory;
using ContratacaoService.Infrastructure.Adapters.Output.Messaging;
using ContratacaoService.Infrastructure.Settings;

namespace ContratacaoService.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
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

        services.AddHttpClient<IPropostaServiceClient, HttpPropostaServiceClient>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["Services:PropostaService"]
                ?? "http://localhost:5001");
        });

        var usarRabbitMQ = configuration.GetValue<bool>("Features:UsarRabbitMQ");

        if (usarRabbitMQ)
        {
            var settings = configuration
                .GetSection("RabbitMQ")
                .Get<RabbitMqSettings>() ?? new RabbitMqSettings();

            services.AddSingleton(settings);

            // Registra com tratamento de erro  RabbitMQ indisponivel nao derruba a API
            services.AddSingleton<IEventPublisher>(sp =>
            {
                try
                {
                    return RabbitMqEventPublisher.CreateAsync(settings).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    var logger = sp.GetRequiredService<ILogger<RabbitMqEventPublisher>>();
                    logger.LogWarning(
                        "RabbitMQ indisponivel  publicacao de eventos desabilitada. Erro: {Erro}",
                        ex.Message);
                    return new NullEventPublisher();
                }
            });
        }

        return services;
    }
}
