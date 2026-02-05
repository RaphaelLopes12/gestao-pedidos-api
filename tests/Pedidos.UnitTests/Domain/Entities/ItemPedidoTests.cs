using FluentAssertions;
using Pedidos.Domain.Entities;

namespace Pedidos.UnitTests.Domain.Entities;

public class ItemPedidoTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveRetornarSucesso()
    {
        var resultado = ItemPedido.Criar(produtoId: 1, quantidade: 5, valorUnitario: 100m);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().NotBeNull();
        resultado.Valor!.ProdutoId.Should().Be(1);
        resultado.Valor.Quantidade.Should().Be(5);
        resultado.Valor.ValorUnitario.Should().Be(100m);
    }

    [Fact]
    public void Criar_ComProdutoIdZero_DeveRetornarFalha()
    {
        var resultado = ItemPedido.Criar(produtoId: 0, quantidade: 5, valorUnitario: 100m);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("ProdutoId");
    }

    [Fact]
    public void Criar_ComProdutoIdNegativo_DeveRetornarFalha()
    {
        var resultado = ItemPedido.Criar(produtoId: -1, quantidade: 5, valorUnitario: 100m);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("ProdutoId");
    }

    [Fact]
    public void Criar_ComQuantidadeZero_DeveRetornarFalha()
    {
        var resultado = ItemPedido.Criar(produtoId: 1, quantidade: 0, valorUnitario: 100m);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("Quantidade");
    }

    [Fact]
    public void Criar_ComQuantidadeNegativa_DeveRetornarFalha()
    {
        var resultado = ItemPedido.Criar(produtoId: 1, quantidade: -5, valorUnitario: 100m);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("Quantidade");
    }

    [Fact]
    public void Criar_ComValorUnitarioZero_DeveRetornarFalha()
    {
        var resultado = ItemPedido.Criar(produtoId: 1, quantidade: 5, valorUnitario: 0m);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("Valor unitário");
    }

    [Fact]
    public void Criar_ComValorUnitarioNegativo_DeveRetornarFalha()
    {
        var resultado = ItemPedido.Criar(produtoId: 1, quantidade: 5, valorUnitario: -100m);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("Valor unitário");
    }

    [Theory]
    [InlineData(1, 100, 100)]
    [InlineData(2, 50, 100)]
    [InlineData(10, 25.50, 255)]
    public void ValorTotal_DeveCalcularCorretamente(int quantidade, decimal valorUnitario, decimal esperado)
    {
        var resultado = ItemPedido.Criar(produtoId: 1, quantidade: quantidade, valorUnitario: valorUnitario);

        resultado.Valor!.ValorTotal.Should().Be(esperado);
    }
}
