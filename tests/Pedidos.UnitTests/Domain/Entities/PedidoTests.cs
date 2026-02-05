using FluentAssertions;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Enums;

namespace Pedidos.UnitTests.Domain.Entities;

public class PedidoTests
{
    private static List<ItemPedido> CriarItensValidos()
    {
        return
        [
            ItemPedido.Criar(1, 2, 100m).Valor!,
            ItemPedido.Criar(2, 1, 50m).Valor!
        ];
    }

    [Fact]
    public void Criar_ComDadosValidos_DeveRetornarSucesso()
    {
        var itens = CriarItensValidos();

        var resultado = Pedido.Criar(pedidoExternoId: 12345, clienteId: 100, itens: itens);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().NotBeNull();
        resultado.Valor!.PedidoExternoId.Should().Be(12345);
        resultado.Valor.ClienteId.Should().Be(100);
        resultado.Valor.Status.Should().Be(StatusPedido.Criado);
        resultado.Valor.Itens.Should().HaveCount(2);
    }

    [Fact]
    public void Criar_ComPedidoExternoIdZero_DeveRetornarFalha()
    {
        var itens = CriarItensValidos();

        var resultado = Pedido.Criar(pedidoExternoId: 0, clienteId: 100, itens: itens);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("PedidoExternoId");
    }

    [Fact]
    public void Criar_ComPedidoExternoIdNegativo_DeveRetornarFalha()
    {
        var itens = CriarItensValidos();

        var resultado = Pedido.Criar(pedidoExternoId: -1, clienteId: 100, itens: itens);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("PedidoExternoId");
    }

    [Fact]
    public void Criar_ComClienteIdZero_DeveRetornarFalha()
    {
        var itens = CriarItensValidos();

        var resultado = Pedido.Criar(pedidoExternoId: 12345, clienteId: 0, itens: itens);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("ClienteId");
    }

    [Fact]
    public void Criar_ComClienteIdNegativo_DeveRetornarFalha()
    {
        var itens = CriarItensValidos();

        var resultado = Pedido.Criar(pedidoExternoId: 12345, clienteId: -1, itens: itens);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("ClienteId");
    }

    [Fact]
    public void Criar_ComListaItensVazia_DeveRetornarFalha()
    {
        var resultado = Pedido.Criar(pedidoExternoId: 12345, clienteId: 100, itens: []);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("pelo menos um item");
    }

    [Fact]
    public void Criar_ComListaItensNula_DeveRetornarFalha()
    {
        var resultado = Pedido.Criar(pedidoExternoId: 12345, clienteId: 100, itens: null!);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("pelo menos um item");
    }

    [Fact]
    public void ValorTotal_DeveSerSomaDosValoresDosItens()
    {
        var itens = CriarItensValidos(); // item1: valor=100, item2: valor=50
        var pedido = Pedido.Criar(12345, 100, itens).Valor!;

        pedido.ValorTotal.Should().Be(150m); // 100 + 50 = 150
    }

    [Fact]
    public void CalcularImposto_DeveAtualizarValorImposto()
    {
        var itens = CriarItensValidos();
        var pedido = Pedido.Criar(12345, 100, itens).Valor!;

        pedido.CalcularImposto(45m); // 150 * 0.3 = 45

        pedido.Imposto.Should().Be(45m);
    }

    [Fact]
    public void MarcarComoProcessado_DeveAtualizarStatusEData()
    {
        var itens = CriarItensValidos();
        var pedido = Pedido.Criar(12345, 100, itens).Valor!;

        pedido.MarcarComoProcessado();

        pedido.Status.Should().Be(StatusPedido.Processado);
        pedido.ProcessadoEm.Should().NotBeNull();
        pedido.ProcessadoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void MarcarComoEnviado_DeveAtualizarStatus()
    {
        var itens = CriarItensValidos();
        var pedido = Pedido.Criar(12345, 100, itens).Valor!;

        pedido.MarcarComoEnviado();

        pedido.Status.Should().Be(StatusPedido.Enviado);
    }

    [Fact]
    public void MarcarComoErro_DeveAtualizarStatus()
    {
        var itens = CriarItensValidos();
        var pedido = Pedido.Criar(12345, 100, itens).Valor!;

        pedido.MarcarComoErro();

        pedido.Status.Should().Be(StatusPedido.Erro);
    }

    [Fact]
    public void Criar_DeveDefinirCriadoEmComoDataAtual()
    {
        var itens = CriarItensValidos();
        var antes = DateTime.UtcNow;

        var pedido = Pedido.Criar(12345, 100, itens).Valor!;

        var depois = DateTime.UtcNow;
        pedido.CriadoEm.Should().BeOnOrAfter(antes);
        pedido.CriadoEm.Should().BeOnOrBefore(depois);
    }
}
