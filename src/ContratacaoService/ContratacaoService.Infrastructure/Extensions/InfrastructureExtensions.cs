using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ContratacaoService.Domain.Ports.Output;
using ContratacaoService.Infrastructure.Adapters.Output.Database;
using ContratacaoService.Infrastructure.Adapters.Output.Http;
using ContratacaoService.Infrastructure.Adapters.Output.InMemory;
using ContratacaoService.Infrastructure.Adapters.Output.Messaging;
using ContratacaoService.Infrastructure.Repositories;
using ContratacaoService.Infrastructure.Workers;
using ContratacaoService.Infrastructure.Messaging;
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
                new NpgsqlConnection(configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<IContratacaoRepository, DapperContratacaoRepository>();
            services.AddScoped<IOutboxRepository, OutboxRepository>();
        }
        else
        {
            services.AddSingleton<IContratacaoRepository, InMemoryContratacaoRepository>();
        }

        services.AddHttpClient<IPropostaServiceClient, HttpPropostaServiceClient>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["Services:PropostaService"] ?? "http://localhost:5001");
        });

        var usarKafka    = configuration.GetValue<bool>("Features:UsarKafka");
        var usarRabbitMQ = configuration.GetValue<bool>("Features:UsarRabbitMQ");

        if (usarKafka)
        {
            services.AddSingleton<IEventPublisher, KafkaEventPublisher>();
        }
        else if (usarRabbitMQ)
        {
            var settings = configuration.GetSection("RabbitMQ").Get<RabbitMqSettings>() ?? new RabbitMqSettings();
            services.AddSingleton(settings);
            services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        }
        else
        {
            services.AddSingleton<IEventPublisher, NullEventPublisher>();
        }

        if (usarKafka || usarRabbitMQ)
        {
            services.AddHostedService<OutboxRelayWorker>();
        }

        return services;
    }
}



