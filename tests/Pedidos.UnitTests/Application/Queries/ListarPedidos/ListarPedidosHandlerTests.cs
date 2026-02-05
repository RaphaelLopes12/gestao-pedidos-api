using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pedidos.Application.Interfaces;
using Pedidos.Application.Queries.ListarPedidos;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Enums;

namespace Pedidos.UnitTests.Application.Queries.ListarPedidos;

public class ListarPedidosHandlerTests
{
    private readonly IRepositorioPedido _repositorio;
    private readonly ILogger<ListarPedidosHandler> _logger;
    private readonly ListarPedidosHandler _handler;

    public ListarPedidosHandlerTests()
    {
        _repositorio = Substitute.For<IRepositorioPedido>();
        _logger = Substitute.For<ILogger<ListarPedidosHandler>>();

        _handler = new ListarPedidosHandler(_repositorio, _logger);
    }

    private static Pedido CriarPedidoValido(int pedidoExternoId)
    {
        var itens = new List<ItemPedido> { ItemPedido.Criar(1, 2, 100m).Valor! };
        return Pedido.Criar(pedidoExternoId, 100, itens).Valor!;
    }

    [Fact]
    public async Task Handle_DeveRetornarListaPaginada()
    {
        var pedidos = new List<Pedido> { CriarPedidoValido(1), CriarPedidoValido(2) };
        _repositorio.ListarAsync(null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((pedidos, 2));

        var resultado = await _handler.Handle(
            new ListarPedidosQuery(null, 1, 20),
            CancellationToken.None);

        resultado.Itens.Should().HaveCount(2);
        resultado.TotalItens.Should().Be(2);
        resultado.Pagina.Should().Be(1);
        resultado.TamanhoPagina.Should().Be(20);
    }

    [Fact]
    public async Task Handle_ComFiltroStatus_DevePassarFiltroParaRepositorio()
    {
        _repositorio.ListarAsync(StatusPedido.Processado, 1, 10, Arg.Any<CancellationToken>())
            .Returns((new List<Pedido>(), 0));

        await _handler.Handle(
            new ListarPedidosQuery(StatusPedido.Processado, 1, 10),
            CancellationToken.None);

        await _repositorio.Received(1).ListarAsync(
            StatusPedido.Processado,
            1,
            10,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DeveCalcularTotalPaginasCorretamente()
    {
        var pedidos = new List<Pedido> { CriarPedidoValido(1) };
        _repositorio.ListarAsync(null, 1, 10, Arg.Any<CancellationToken>())
            .Returns((pedidos, 25));

        var resultado = await _handler.Handle(
            new ListarPedidosQuery(null, 1, 10),
            CancellationToken.None);

        resultado.TotalPaginas.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ComListaVazia_DeveRetornarResultadoVazio()
    {
        _repositorio.ListarAsync(null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<Pedido>(), 0));

        var resultado = await _handler.Handle(
            new ListarPedidosQuery(null, 1, 20),
            CancellationToken.None);

        resultado.Itens.Should().BeEmpty();
        resultado.TotalItens.Should().Be(0);
        resultado.TotalPaginas.Should().Be(0);
    }

    [Fact]
    public async Task Handle_DeveMapearItensCorretamente()
    {
        var pedidos = new List<Pedido> { CriarPedidoValido(12345) };
        _repositorio.ListarAsync(null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((pedidos, 1));

        var resultado = await _handler.Handle(
            new ListarPedidosQuery(null, 1, 20),
            CancellationToken.None);

        resultado.Itens[0].PedidoId.Should().Be(12345);
        resultado.Itens[0].Itens.Should().HaveCount(1);
    }
}
