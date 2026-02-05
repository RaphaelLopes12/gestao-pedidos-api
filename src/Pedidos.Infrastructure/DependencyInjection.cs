using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pedidos.Application.Interfaces;
using Pedidos.Domain.Services;
using Pedidos.Domain.Services.Interfaces;
using Pedidos.Infrastructure.Messaging;
using Pedidos.Infrastructure.Messaging.Consumers;
using Pedidos.Infrastructure.Options;
using Pedidos.Infrastructure.Persistence;
using Pedidos.Infrastructure.Repositories;

namespace Pedidos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<PedidosDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(PedidosDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

        // Feature Flags
        services.Configure<FeatureFlagOptions>(
            configuration.GetSection(FeatureFlagOptions.SectionName));

        // RabbitMQ Options
        services.Configure<RabbitMqOptions>(
            configuration.GetSection(RabbitMqOptions.SectionName));

        // Repositórios
        services.AddScoped<IRepositorioPedido, RepositorioPedido>();

        // Publicador de Eventos
        services.AddScoped<IPublicadorEventos, PublicadorEventosRabbitMq>();

        // Calculadora de Imposto (Strategy Pattern com Feature Flag)
        services.AddScoped<ICalculadoraImposto>(sp =>
        {
            var featureFlags = configuration
                .GetSection(FeatureFlagOptions.SectionName)
                .Get<FeatureFlagOptions>() ?? new FeatureFlagOptions();

            return featureFlags.UsarReformaTributaria
                ? new CalculadoraImpostoReforma()
                : new CalculadoraImpostoAtual();
        });

        // MassTransit com RabbitMQ
        var rabbitMqOptions = configuration
            .GetSection(RabbitMqOptions.SectionName)
            .Get<RabbitMqOptions>() ?? new RabbitMqOptions();

        services.AddMassTransit(busConfig =>
        {
            busConfig.AddConsumer<ProcessarPedidoConsumer>();

            busConfig.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqOptions.Host, rabbitMqOptions.Port, "/", h =>
                {
                    h.Username(rabbitMqOptions.Username);
                    h.Password(rabbitMqOptions.Password);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
