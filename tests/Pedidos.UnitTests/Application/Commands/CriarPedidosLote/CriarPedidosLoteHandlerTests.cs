using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pedidos.Application.Commands.CriarPedidosLote;
using Pedidos.Application.DTOs.Requests;
using Pedidos.Application.Events;
using Pedidos.Application.Interfaces;
using Pedidos.Domain.Entities;

namespace Pedidos.UnitTests.Application.Commands.CriarPedidosLote;

public class CriarPedidosLoteHandlerTests
{
    private readonly IRepositorioPedido _repositorio;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CriarPedidosLoteHandler> _logger;
    private readonly CriarPedidosLoteHandler _handler;

    public CriarPedidosLoteHandlerTests()
    {
        _repositorio = Substitute.For<IRepositorioPedido>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _logger = Substitute.For<ILogger<CriarPedidosLoteHandler>>();

        _handler = new CriarPedidosLoteHandler(_repositorio, _publishEndpoint, _logger);
    }

    private static CriarPedidoRequest CriarPedidoRequestValido(int pedidoId = 1)
    {
        return new CriarPedidoRequest(
            PedidoId: pedidoId,
            ClienteId: 100,
            Itens: [new ItemPedidoRequest(1, 2, 100m)]
        );
    }

    [Fact]
    public async Task Handle_ComLoteValido_DeveRetornarSucesso()
    {
        var command = new CriarPedidosLoteCommand([
            CriarPedidoRequestValido(1),
            CriarPedidoRequestValido(2)
        ]);
        _repositorio.ObterPorPedidoExternoIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((Pedido?)null);
        _repositorio.AdicionarAsync(Arg.Any<Pedido>(), Arg.Any<CancellationToken>())
            .Returns(1, 2);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.QuantidadePedidos.Should().Be(2);
        resultado.Valor.Status.Should().Be("EmProcessamento");
    }

    [Fact]
    public async Task Handle_ComPedidoDuplicado_DeveCriarApenasNovos()
    {
        var pedidoExistente = Pedido.Criar(1, 100, [ItemPedido.Criar(1, 1, 100m).Valor!]).Valor!;
        var command = new CriarPedidosLoteCommand([
            CriarPedidoRequestValido(1),
            CriarPedidoRequestValido(2)
        ]);
        _repositorio.ObterPorPedidoExternoIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(pedidoExistente);
        _repositorio.ObterPorPedidoExternoIdAsync(2, Arg.Any<CancellationToken>())
            .Returns((Pedido?)null);
        _repositorio.AdicionarAsync(Arg.Any<Pedido>(), Arg.Any<CancellationToken>())
            .Returns(2);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.QuantidadePedidos.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ComTodosDuplicados_DeveRetornarFalha()
    {
        var pedidoExistente = Pedido.Criar(1, 100, [ItemPedido.Criar(1, 1, 100m).Valor!]).Valor!;
        var command = new CriarPedidosLoteCommand([CriarPedidoRequestValido(1)]);
        _repositorio.ObterPorPedidoExternoIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(pedidoExistente);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("Nenhum pedido foi criado");
    }

    [Fact]
    public async Task Handle_DevePublicarEventoParaCadaPedidoCriado()
    {
        var command = new CriarPedidosLoteCommand([
            CriarPedidoRequestValido(1),
            CriarPedidoRequestValido(2)
        ]);
        _repositorio.ObterPorPedidoExternoIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((Pedido?)null);
        _repositorio.AdicionarAsync(Arg.Any<Pedido>(), Arg.Any<CancellationToken>())
            .Returns(1, 2);

        await _handler.Handle(command, CancellationToken.None);

        await _publishEndpoint.Received(2).Publish(
            Arg.Any<ProcessarPedidoLoteEvento>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ComItemInvalido_DeveIgnorarPedidoComErro()
    {
        var pedidoComItemInvalido = new CriarPedidoRequest(
            PedidoId: 1,
            ClienteId: 100,
            Itens: [new ItemPedidoRequest(0, 2, 100m)]
        );
        var command = new CriarPedidosLoteCommand([
            pedidoComItemInvalido,
            CriarPedidoRequestValido(2)
        ]);
        _repositorio.ObterPorPedidoExternoIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((Pedido?)null);
        _repositorio.AdicionarAsync(Arg.Any<Pedido>(), Arg.Any<CancellationToken>())
            .Returns(2);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.QuantidadePedidos.Should().Be(1);
    }

    [Fact]
    public async Task Handle_DeveRetornarLoteIdUnico()
    {
        var command = new CriarPedidosLoteCommand([CriarPedidoRequestValido(1)]);
        _repositorio.ObterPorPedidoExternoIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((Pedido?)null);
        _repositorio.AdicionarAsync(Arg.Any<Pedido>(), Arg.Any<CancellationToken>())
            .Returns(1);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Valor!.LoteId.Should().NotBeEmpty();
    }
}
