using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pedidos.Application.DTOs.Requests;
using Pedidos.Application.DTOs.Responses;
using Pedidos.Domain.Services;
using Pedidos.Domain.Services.Interfaces;
using Pedidos.Infrastructure.Persistence;
using Testcontainers.MsSql;
using Testcontainers.RabbitMq;

namespace Pedidos.IntegrationTests;

public class FeatureFlagTests : IAsyncLifetime
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

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            _sqlContainer.DisposeAsync().AsTask(),
            _rabbitContainer.DisposeAsync().AsTask());
    }

    private WebApplicationFactory<Program> CriarFactory(ICalculadoraImposto calculadora)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<PedidosDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

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

                    var calculadoraDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ICalculadoraImposto));
                    if (calculadoraDescriptor != null)
                        services.Remove(calculadoraDescriptor);

                    services.AddScoped<ICalculadoraImposto>(_ => calculadora);

                    using var scope = services.BuildServiceProvider().CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<PedidosDbContext>();
                    db.Database.EnsureCreated();
                });

                builder.UseEnvironment("Testing");
            });
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

    [Fact]
    public async Task CriarPedido_ComCalculadoraAtual_DeveCalcularImposto30Porcento()
    {
        await using var factory = CriarFactory(new CalculadoraImpostoAtual());
        var client = factory.CreateClient();

        var request = new CriarPedidoRequest(
            PedidoExternoId: 80001,
            ClienteId: 100,
            Itens: [new ItemPedidoRequest(1, 1, 100m)]);

        var response = await client.PostAsJsonAsync("/api/v1/pedidos", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var resultado = await response.Content.ReadFromJsonAsync<CriarPedidoResponse>();
        resultado.Should().NotBeNull();
        resultado!.ValorTotal.Should().Be(100m);
        resultado.Imposto.Should().Be(30m);
    }

    [Fact]
    public async Task CriarPedido_ComCalculadoraReforma_DeveCalcularImposto20Porcento()
    {
        await using var factory = CriarFactory(new CalculadoraImpostoReforma());
        var client = factory.CreateClient();

        var request = new CriarPedidoRequest(
            PedidoExternoId: 80002,
            ClienteId: 100,
            Itens: [new ItemPedidoRequest(1, 1, 100m)]);

        var response = await client.PostAsJsonAsync("/api/v1/pedidos", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var resultado = await response.Content.ReadFromJsonAsync<CriarPedidoResponse>();
        resultado.Should().NotBeNull();
        resultado!.ValorTotal.Should().Be(100m);
        resultado.Imposto.Should().Be(20m);
    }
}
