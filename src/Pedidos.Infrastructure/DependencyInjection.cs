using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pedidos.Application.Interfaces;
using Pedidos.Domain.Services;
using Pedidos.Domain.Services.Interfaces;
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

        // Repositórios
        services.AddScoped<IRepositorioPedido, RepositorioPedido>();

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

        return services;
    }
}
