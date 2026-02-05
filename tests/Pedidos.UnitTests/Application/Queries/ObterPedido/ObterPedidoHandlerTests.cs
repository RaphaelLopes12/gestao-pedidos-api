using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pedidos.Application.Interfaces;
using Pedidos.Application.Queries.ObterPedido;
using Pedidos.Domain.Entities;

namespace Pedidos.UnitTests.Application.Queries.ObterPedido;

public class ObterPedidoHandlerTests
{
    private readonly IRepositorioPedido _repositorio;
    private readonly ILogger<ObterPedidoHandler> _logger;
    private readonly ObterPedidoHandler _handler;

    public ObterPedidoHandlerTests()
    {
        _repositorio = Substitute.For<IRepositorioPedido>();
        _logger = Substitute.For<ILogger<ObterPedidoHandler>>();

        _handler = new ObterPedidoHandler(_repositorio, _logger);
    }

    private static Pedido CriarPedidoValido()
    {
        var itens = new List<ItemPedido> { ItemPedido.Criar(1, 2, 100m).Valor! };
        return Pedido.Criar(12345, 100, itens).Valor!;
    }

    [Fact]
    public async Task Handle_ComPedidoExistente_DeveRetornarSucesso()
    {
        var pedido = CriarPedidoValido();
        _repositorio.ObterPorIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(pedido);

        var resultado = await _handler.Handle(new ObterPedidoQuery(1), CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().NotBeNull();
        resultado.Valor!.PedidoExternoId.Should().Be(12345);
        resultado.Valor.ClienteId.Should().Be(100);
    }

    [Fact]
    public async Task Handle_ComPedidoInexistente_DeveRetornarFalha()
    {
        _repositorio.ObterPorIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((Pedido?)null);

        var resultado = await _handler.Handle(new ObterPedidoQuery(999), CancellationToken.None);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("não encontrado");
    }

    [Fact]
    public async Task Handle_DeveRetornarItensCorretamente()
    {
        var pedido = CriarPedidoValido();
        _repositorio.ObterPorIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(pedido);

        var resultado = await _handler.Handle(new ObterPedidoQuery(1), CancellationToken.None);

        resultado.Valor!.Itens.Should().HaveCount(1);
        resultado.Valor.Itens[0].ProdutoId.Should().Be(1);
        resultado.Valor.Itens[0].Quantidade.Should().Be(2);
        resultado.Valor.Itens[0].ValorUnitario.Should().Be(100m);
    }

    [Fact]
    public async Task Handle_DeveRetornarValorTotalCorreto()
    {
        var pedido = CriarPedidoValido();
        _repositorio.ObterPorIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(pedido);

        var resultado = await _handler.Handle(new ObterPedidoQuery(1), CancellationToken.None);

        resultado.Valor!.ValorTotal.Should().Be(200m);
    }

    [Fact]
    public async Task Handle_DeveRetornarStatusCorreto()
    {
        var pedido = CriarPedidoValido();
        _repositorio.ObterPorIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(pedido);

        var resultado = await _handler.Handle(new ObterPedidoQuery(1), CancellationToken.None);

        resultado.Valor!.Status.Should().Be("Criado");
    }
}
