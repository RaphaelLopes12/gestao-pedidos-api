using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Pedidos.Application.DTOs.Requests;
using Pedidos.Application.DTOs.Responses;
using Pedidos.IntegrationTests.Fixtures;

namespace Pedidos.IntegrationTests;

[Collection("IntegrationTests")]
public class PedidosApiTests
{
    private readonly HttpClient _client;

    public PedidosApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CriarPedido_ComDadosValidos_DeveRetornar201()
    {
        var request = new CriarPedidoRequest(
            PedidoId: 12345,
            ClienteId: 100,
            Itens:
            [
                new ItemPedidoRequest(ProdutoId: 1, Quantidade: 2, Valor: 100m)
            ]);

        var response = await _client.PostAsJsonAsync("/api/v1/pedidos", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var resultado = await response.Content.ReadFromJsonAsync<CriarPedidoResponse>();
        resultado.Should().NotBeNull();
        resultado!.Id.Should().BeGreaterThan(0);
        resultado.Status.Should().Be("Processado");
    }

    [Fact]
    public async Task CriarPedido_ComPedidoIdDuplicado_DeveRetornar409()
    {
        var request = new CriarPedidoRequest(
            PedidoId: 99999,
            ClienteId: 100,
            Itens: [new ItemPedidoRequest(1, 1, 100m)]);

        await _client.PostAsJsonAsync("/api/v1/pedidos", request);

        var duplicateResponse = await _client.PostAsJsonAsync("/api/v1/pedidos", request);

        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CriarPedido_SemItens_DeveRetornar400()
    {
        var request = new CriarPedidoRequest(
            PedidoId: 11111,
            ClienteId: 100,
            Itens: []);

        var response = await _client.PostAsJsonAsync("/api/v1/pedidos", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObterPedido_ComIdExistente_DeveRetornar200()
    {
        var request = new CriarPedidoRequest(
            PedidoId: 22222,
            ClienteId: 100,
            Itens: [new ItemPedidoRequest(1, 1, 100m)]);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/pedidos", request);
        var pedidoCriado = await createResponse.Content.ReadFromJsonAsync<CriarPedidoResponse>();

        var response = await _client.GetAsync($"/api/v1/pedidos/{pedidoCriado!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pedido = await response.Content.ReadFromJsonAsync<PedidoResponse>();
        pedido.Should().NotBeNull();
        pedido!.PedidoId.Should().Be(22222);
    }

    [Fact]
    public async Task ObterPedido_ComIdInexistente_DeveRetornar404()
    {
        var response = await _client.GetAsync("/api/v1/pedidos/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListarPedidos_DeveRetornar200ComPaginacao()
    {
        var request = new CriarPedidoRequest(
            PedidoId: 33333,
            ClienteId: 100,
            Itens: [new ItemPedidoRequest(1, 1, 100m)]);

        await _client.PostAsJsonAsync("/api/v1/pedidos", request);

        var response = await _client.GetAsync("/api/v1/pedidos?pagina=1&tamanhoPagina=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resultado = await response.Content.ReadFromJsonAsync<PaginacaoResponse<PedidoResponse>>();
        resultado.Should().NotBeNull();
        resultado!.Pagina.Should().Be(1);
        resultado.TamanhoPagina.Should().Be(10);
    }

    [Fact]
    public async Task Health_DeveRetornar200()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
