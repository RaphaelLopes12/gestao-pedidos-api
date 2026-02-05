using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pedidos.Infrastructure.Persistence;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace Pedidos.IntegrationTests.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private readonly RabbitMqContainer _rabbitContainer = new RabbitMqBuilder("rabbitmq:3-management")
        .WithUsername("testuser")
        .WithPassword("testpass")
        .Build();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _sqlContainer.StartAsync(),
            _rabbitContainer.StartAsync());
    }

    public new async Task DisposeAsync()
    {
        await Task.WhenAll(
            _sqlContainer.DisposeAsync().AsTask(),
            _rabbitContainer.DisposeAsync().AsTask());
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<PedidosDbContext>));
            if (dbDescriptor != null)
                services.Remove(dbDescriptor);

            services.AddDbContext<PedidosDbContext>(options =>
                options.UseSqlServer(_sqlContainer.GetConnectionString()));

            RemoverMassTransit(services);

            services.AddMassTransit(busConfig =>
            {
                busConfig.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(_rabbitContainer.Hostname, _rabbitContainer.GetMappedPublicPort(5672), "/", h =>
                    {
                        h.Username("testuser");
                        h.Password("testpass");
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PedidosDbContext>();
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }

    private static void RemoverMassTransit(IServiceCollection services)
    {
        var massTransitDescriptors = services
            .Where(d => d.ServiceType.FullName?.Contains("MassTransit") == true ||
                        d.ImplementationType?.FullName?.Contains("MassTransit") == true)
            .ToList();

        foreach (var descriptor in massTransitDescriptors)
        {
            services.Remove(descriptor);
        }
    }
}
