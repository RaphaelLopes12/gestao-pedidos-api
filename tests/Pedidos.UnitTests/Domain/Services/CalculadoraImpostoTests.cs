using FluentAssertions;
using Pedidos.Domain.Constants;
using Pedidos.Domain.Services;

namespace Pedidos.UnitTests.Domain.Services;

public class CalculadoraImpostoTests
{
    [Theory]
    [InlineData(100, 30)]
    [InlineData(250, 75)]
    [InlineData(1000, 300)]
    [InlineData(0, 0)]
    public void CalculadoraImpostoAtual_DeveAplicar30Porcento(decimal valorTotal, decimal esperado)
    {
        var calculadora = new CalculadoraImpostoAtual();

        var resultado = calculadora.Calcular(valorTotal);

        resultado.Should().Be(esperado);
    }

    [Theory]
    [InlineData(100, 20)]
    [InlineData(250, 50)]
    [InlineData(1000, 200)]
    [InlineData(0, 0)]
    public void CalculadoraImpostoReforma_DeveAplicar20Porcento(decimal valorTotal, decimal esperado)
    {
        var calculadora = new CalculadoraImpostoReforma();

        var resultado = calculadora.Calcular(valorTotal);

        resultado.Should().Be(esperado);
    }

    [Fact]
    public void TaxaAtual_DeveSerTrintaPorcento()
    {
        TaxasImposto.TaxaAtual.Should().Be(0.30m);
    }

    [Fact]
    public void TaxaReformaTributaria_DeveSerVintePorcento()
    {
        TaxasImposto.TaxaReformaTributaria.Should().Be(0.20m);
    }

    [Fact]
    public void CalculadoraImpostoAtual_ComValorDecimal_DeveCalcularCorretamente()
    {
        var calculadora = new CalculadoraImpostoAtual();

        var resultado = calculadora.Calcular(333.33m);

        resultado.Should().Be(99.999m);
    }

    [Fact]
    public void CalculadoraImpostoReforma_ComValorDecimal_DeveCalcularCorretamente()
    {
        var calculadora = new CalculadoraImpostoReforma();

        var resultado = calculadora.Calcular(333.33m);

        resultado.Should().Be(66.666m);
    }
}
