using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pedidos.Application.Commands.CriarPedido;
using Pedidos.Application.DTOs.Requests;
using Pedidos.Application.Events;
using Pedidos.Application.Interfaces;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Services.Interfaces;

namespace Pedidos.UnitTests.Application.Commands.CriarPedido;

public class CriarPedidoHandlerTests
{
    private readonly IRepositorioPedido _repositorio;
    private readonly ICalculadoraImposto _calculadora;
    private readonly IPublicadorEventos _publicador;
    private readonly ILogger<CriarPedidoHandler> _logger;
    private readonly CriarPedidoHandler _handler;

    public CriarPedidoHandlerTests()
    {
        _repositorio = Substitute.For<IRepositorioPedido>();
        _calculadora = Substitute.For<ICalculadoraImposto>();
        _publicador = Substitute.For<IPublicadorEventos>();
        _logger = Substitute.For<ILogger<CriarPedidoHandler>>();

        _handler = new CriarPedidoHandler(_repositorio, _calculadora, _publicador, _logger);
    }

    private static CriarPedidoCommand CriarCommandValido()
    {
        return new CriarPedidoCommand(
            PedidoExternoId: 12345,
            ClienteId: 100,
            Itens: [new ItemPedidoRequest(1, 2, 100m)]
        );
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveRetornarSucesso()
    {
        var command = CriarCommandValido();
        _repositorio.ObterPorPedidoExternoIdAsync(command.PedidoExternoId, Arg.Any<CancellationToken>())
            .Returns((Pedido?)null);
        _repositorio.AdicionarAsync(Arg.Any<Pedido>(), Arg.Any<CancellationToken>())
            .Returns(1);
        _calculadora.Calcular(Arg.Any<decimal>()).Returns(60m);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().NotBeNull();
        resultado.Valor!.Id.Should().Be(1);
        resultado.Valor.PedidoExternoId.Should().Be(12345);
    }

    [Fact]
    public async Task Handle_ComPedidoDuplicado_DeveRetornarFalha()
    {
        var command = CriarCommandValido();
        var pedidoExistente = Pedido.Criar(12345, 100, [ItemPedido.Criar(1, 1, 100m).Valor!]).Valor!;
        _repositorio.ObterPorPedidoExternoIdAsync(command.PedidoExternoId, Arg.Any<CancellationToken>())
            .Returns(pedidoExistente);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("já existe");
    }

    [Fact]
    public async Task Handle_DeveCalcularImpostoCorretamente()
    {
        var command = CriarCommandValido();
        _repositorio.ObterPorPedidoExternoIdAsync(command.PedidoExternoId, Arg.Any<CancellationToken>())
            .Returns((Pedido?)null);
        _repositorio.AdicionarAsync(Arg.Any<Pedido>(), Arg.Any<CancellationToken>())
            .Returns(1);
        _calculadora.Calcular(200m).Returns(60m);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Valor!.Imposto.Should().Be(60m);
        _calculadora.Received(1).Calcular(200m);
    }

    [Fact]
    public async Task Handle_DevePublicarEventoAposProcessamento()
    {
        var command = CriarCommandValido();
        _repositorio.ObterPorPedidoExternoIdAsync(command.PedidoExternoId, Arg.Any<CancellationToken>())
            .Returns((Pedido?)null);
        _repositorio.AdicionarAsync(Arg.Any<Pedido>(), Arg.Any<CancellationToken>())
            .Returns(1);
        _calculadora.Calcular(Arg.Any<decimal>()).Returns(60m);

        await _handler.Handle(command, CancellationToken.None);

        await _publicador.Received(1).PublicarPedidoProcessadoAsync(
            Arg.Is<PedidoProcessadoEvento>(e => e.PedidoId == 1 && e.PedidoExternoId == 12345),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ComItemInvalido_DeveRetornarFalha()
    {
        var command = new CriarPedidoCommand(
            PedidoExternoId: 12345,
            ClienteId: 100,
            Itens: [new ItemPedidoRequest(0, 2, 100m)]
        );
        _repositorio.ObterPorPedidoExternoIdAsync(command.PedidoExternoId, Arg.Any<CancellationToken>())
            .Returns((Pedido?)null);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("ProdutoId");
    }

    [Fact]
    public async Task Handle_DevePersistirPedidoComStatusProcessado()
    {
        var command = CriarCommandValido();
        Pedido? pedidoSalvo = null;
        _repositorio.ObterPorPedidoExternoIdAsync(command.PedidoExternoId, Arg.Any<CancellationToken>())
            .Returns((Pedido?)null);
        _repositorio.AdicionarAsync(Arg.Any<Pedido>(), Arg.Any<CancellationToken>())
            .Returns(1);
        _repositorio.AtualizarAsync(Arg.Do<Pedido>(p => pedidoSalvo = p), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _calculadora.Calcular(Arg.Any<decimal>()).Returns(60m);

        await _handler.Handle(command, CancellationToken.None);

        pedidoSalvo.Should().NotBeNull();
        pedidoSalvo!.Status.Should().Be(Pedidos.Domain.Enums.StatusPedido.Processado);
    }
}
