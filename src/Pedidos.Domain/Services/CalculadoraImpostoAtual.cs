using Pedidos.Domain.Constants;
using Pedidos.Domain.Services.Interfaces;

namespace Pedidos.Domain.Services;

public sealed class CalculadoraImpostoAtual : ICalculadoraImposto
{
    public decimal Calcular(decimal valorTotal) => valorTotal * TaxasImposto.TaxaAtual;
}
