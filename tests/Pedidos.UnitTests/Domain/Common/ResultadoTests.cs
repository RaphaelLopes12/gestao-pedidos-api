using FluentAssertions;
using Pedidos.Domain.Common;

namespace Pedidos.UnitTests.Domain.Common;

public class ResultadoTests
{
    [Fact]
    public void Ok_DeveRetornarSucesso()
    {
        var resultado = Resultado.Ok();

        resultado.Sucesso.Should().BeTrue();
        resultado.Falhou.Should().BeFalse();
        resultado.Erro.Should().BeEmpty();
    }

    [Fact]
    public void Falha_DeveRetornarFalhaComMensagem()
    {
        var resultado = Resultado.Falha("Erro de teste");

        resultado.Sucesso.Should().BeFalse();
        resultado.Falhou.Should().BeTrue();
        resultado.Erro.Should().Be("Erro de teste");
    }

    [Fact]
    public void ResultadoGenerico_Ok_DeveRetornarSucessoComValor()
    {
        var resultado = Resultado<string>.Ok("valor teste");

        resultado.Sucesso.Should().BeTrue();
        resultado.Falhou.Should().BeFalse();
        resultado.Valor.Should().Be("valor teste");
        resultado.Erro.Should().BeEmpty();
    }

    [Fact]
    public void ResultadoGenerico_Falha_DeveRetornarFalhaSemValor()
    {
        var resultado = Resultado<string>.Falha("Erro de teste");

        resultado.Sucesso.Should().BeFalse();
        resultado.Falhou.Should().BeTrue();
        resultado.Valor.Should().BeNull();
        resultado.Erro.Should().Be("Erro de teste");
    }

    [Fact]
    public void ResultadoGenerico_Ok_ComTipoComplexo_DeveRetornarValor()
    {
        var objeto = new { Id = 1, Nome = "Teste" };
        var resultado = Resultado<object>.Ok(objeto);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().NotBeNull();
    }

    [Fact]
    public void ResultadoGenerico_Ok_ComTipoNumerico_DeveRetornarValor()
    {
        var resultado = Resultado<int>.Ok(42);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().Be(42);
    }

    [Fact]
    public void ResultadoGenerico_Falha_ComTipoNumerico_DeveRetornarDefault()
    {
        var resultado = Resultado<int>.Falha("Erro");

        resultado.Falhou.Should().BeTrue();
        resultado.Valor.Should().Be(0);
    }
}
