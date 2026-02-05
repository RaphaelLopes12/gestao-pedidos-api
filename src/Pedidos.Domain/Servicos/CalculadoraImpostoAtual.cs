using Pedidos.Domain.Constantes;
using Pedidos.Domain.Servicos.Interfaces;

namespace Pedidos.Domain.Servicos;

public sealed class CalculadoraImpostoAtual : ICalculadoraImposto
{
    public decimal Calcular(decimal valorTotal) => valorTotal * TaxasImposto.TaxaAtual;
}
