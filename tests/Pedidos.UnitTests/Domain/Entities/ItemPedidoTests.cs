using FluentAssertions;
using Pedidos.Domain.Entities;

namespace Pedidos.UnitTests.Domain.Entities;

public class ItemPedidoTests
{
    [Fact]
    public void Criar_ComDadosValidos_DeveRetornarSucesso()
    {
        var resultado = ItemPedido.Criar(produtoId: 1, quantidade: 5, valor: 100m);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().NotBeNull();
        resultado.Valor!.ProdutoId.Should().Be(1);
        resultado.Valor.Quantidade.Should().Be(5);
        resultado.Valor.Valor.Should().Be(100m);
    }

    [Fact]
    public void Criar_ComProdutoIdZero_DeveRetornarFalha()
    {
        var resultado = ItemPedido.Criar(produtoId: 0, quantidade: 5, valor: 100m);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("ProdutoId");
    }

    [Fact]
    public void Criar_ComProdutoIdNegativo_DeveRetornarFalha()
    {
        var resultado = ItemPedido.Criar(produtoId: -1, quantidade: 5, valor: 100m);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("ProdutoId");
    }

    [Fact]
    public void Criar_ComQuantidadeZero_DeveRetornarFalha()
    {
        var resultado = ItemPedido.Criar(produtoId: 1, quantidade: 0, valor: 100m);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("Quantidade");
    }

    [Fact]
    public void Criar_ComQuantidadeNegativa_DeveRetornarFalha()
    {
        var resultado = ItemPedido.Criar(produtoId: 1, quantidade: -5, valor: 100m);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("Quantidade");
    }

    [Fact]
    public void Criar_ComValorZero_DeveRetornarFalha()
    {
        var resultado = ItemPedido.Criar(produtoId: 1, quantidade: 5, valor: 0m);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("Valor");
    }

    [Fact]
    public void Criar_ComValorNegativo_DeveRetornarFalha()
    {
        var resultado = ItemPedido.Criar(produtoId: 1, quantidade: 5, valor: -100m);

        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Contain("Valor");
    }
}
