using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Pedidos.Application.DTOs.Requests;
using Pedidos.Application.DTOs.Responses;
using Pedidos.IntegrationTests.Fixtures;

namespace Pedidos.IntegrationTests;

[Collection("IntegrationTests")]
public class PedidosLoteApiTests
{
    private readonly HttpClient _client;

    public PedidosLoteApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CriarLote_ComDadosValidos_DeveRetornar202()
    {
        var request = new CriarPedidosLoteRequest(
        [
            new CriarPedidoRequest(50001, 100, [new ItemPedidoRequest(1, 1, 100m)]),
            new CriarPedidoRequest(50002, 101, [new ItemPedidoRequest(2, 2, 100m)])
        ]);

        var response = await _client.PostAsJsonAsync("/api/v1/pedidos/lote", request);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var resultado = await response.Content.ReadFromJsonAsync<CriarLoteResponse>();
        resultado.Should().NotBeNull();
        resultado!.QuantidadePedidos.Should().Be(2);
        resultado.Status.Should().Be("EmProcessamento");
        resultado.LoteId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CriarLote_ComListaVazia_DeveRetornar400()
    {
        var request = new CriarPedidosLoteRequest([]);

        var response = await _client.PostAsJsonAsync("/api/v1/pedidos/lote", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CriarLote_ComPedidoSemItens_DeveRetornar400()
    {
        var request = new CriarPedidosLoteRequest(
        [
            new CriarPedidoRequest(60001, 100, [])
        ]);

        var response = await _client.PostAsJsonAsync("/api/v1/pedidos/lote", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CriarLote_ComPedidoIdDuplicados_DevePersistirApenasUmaVez()
    {
        var request = new CriarPedidosLoteRequest(
        [
            new CriarPedidoRequest(70001, 100, [new ItemPedidoRequest(1, 1, 100m)]),
            new CriarPedidoRequest(70001, 100, [new ItemPedidoRequest(1, 1, 100m)])
        ]);

        var response = await _client.PostAsJsonAsync("/api/v1/pedidos/lote", request);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var resultado = await response.Content.ReadFromJsonAsync<CriarLoteResponse>();
        resultado!.QuantidadePedidos.Should().Be(1);
    }
}
