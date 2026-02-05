using Bogus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pedidos.Application.Commands.CriarPedido;
using Pedidos.Application.DTOs.Requests;
using Pedidos.Application.Events;
using Pedidos.Application.Interfaces;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Enums;
using Pedidos.Domain.Services.Interfaces;
using Pedidos.UnitTests.Fakers;

namespace Pedidos.UnitTests.Application.Commands.CriarPedido;

public class CriarPedidoHandlerTests
{
    private readonly IRepositorioPedido _repositorio;
    private readonly ICalculadoraImposto _calculadora;
    private readonly IPublicadorEventos _publicador;
    private readonly ILogger<CriarPedidoHandler> _logger;
    private readonly CriarPedidoHandler _handler;
    private readonly Faker _faker;

    public CriarPedidoHandlerTests()
    {
        _repositorio = Substitute.For<IRepositorioPedido>();
        _calculadora = Substitute.For<ICalculadoraImposto>();
        _publicador = Substitute.For<IPublicadorEventos>();
        _logger = Substitute.For<ILogger<CriarPedidoHandler>>();
        _faker = new Faker("pt_BR");

        _handler = new CriarPedidoHandler(_repositorio, _calculadora, _publicador, _logger);
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveRetornarSucesso()
    {
        var command = PedidoFaker.GerarCommandValido();
        _repositorio.ObterPorPedidoExternoIdAsync(command.PedidoExternoId, Arg.Any<CancellationToken>())
            .Returns((Pedido?)null);
        _repositorio.AdicionarAsync(Arg.Any<Pedido>(), Arg.Any<CancellationToken>())
            .Returns(1);
        _calculadora.Calcular(Arg.Any<decimal>()).Returns(60m);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().NotBeNull();
        resultado.Valor!.Id.Should().Be(1);
        resultado.Valor.PedidoExternoId.Should().Be(command.PedidoExternoId);
    }

    [Fact]
    public async Task Handle_ComPedidoDuplicado_DeveRetornarFalha()
    {
        var pedidoExternoId = _faker.Random.Int(1, 99999);
        var command = PedidoFaker.GerarCommandComPedidoExternoId(pedidoExternoId);
        var pedidoExistente = PedidoFaker.GerarPedidoComPedidoExternoId(pedidoExternoId);
        _repositorio.ObterPorPedidoExternoIdAsync(command.PedidoExternoId, Arg.Any<CancellationToken>())
            .Returns(pedidoExistente);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("já existe");
    }

    [Fact]
    public async Task Handle_DeveCalcularImpostoCorretamente()
    {
        var command = new CriarPedidoCommand(
            PedidoExternoId: _faker.Random.Int(1, 99999),
            ClienteId: _faker.Random.Int(1, 1000),
            Itens: [new ItemPedidoRequest(1, 2, 100m)]
        );
        var valorTotalEsperado = 200m;
        var impostoEsperado = 60m;

        _repositorio.ObterPorPedidoExternoIdAsync(command.PedidoExternoId, Arg.Any<CancellationToken>())
            .Returns((Pedido?)null);
        _repositorio.AdicionarAsync(Arg.Any<Pedido>(), Arg.Any<CancellationToken>())
            .Returns(1);
        _calculadora.Calcular(valorTotalEsperado).Returns(impostoEsperado);

        var resultado = await _handler.Handle(command, CancellationToken.None);

        resultado.Valor!.Imposto.Should().Be(impostoEsperado);
        _calculadora.Received(1).Calcular(valorTotalEsperado);
    }

    [Fact]
    public async Task Handle_DevePublicarEventoAposProcessamento()
    {
        var command = PedidoFaker.GerarCommandValido();
        _repositorio.ObterPorPedidoExternoIdAsync(command.PedidoExternoId, Arg.Any<CancellationToken>())
            .Returns((Pedido?)null);
        _repositorio.AdicionarAsync(Arg.Any<Pedido>(), Arg.Any<CancellationToken>())
            .Returns(1);
        _calculadora.Calcular(Arg.Any<decimal>()).Returns(60m);

        await _handler.Handle(command, CancellationToken.None);

        await _publicador.Received(1).PublicarPedidoProcessadoAsync(
            Arg.Is<PedidoProcessadoEvento>(e => e.PedidoId == 1 && e.PedidoExternoId == command.PedidoExternoId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ComItemInvalido_DeveRetornarFalha()
    {
        var command = new CriarPedidoCommand(
            PedidoExternoId: _faker.Random.Int(1, 99999),
            ClienteId: _faker.Random.Int(1, 1000),
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
        var command = PedidoFaker.GerarCommandValido();
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
        pedidoSalvo!.Status.Should().Be(StatusPedido.Processado);
    }
}
