using Pedidos.Domain.Constantes;
using Pedidos.Domain.Servicos.Interfaces;

namespace Pedidos.Domain.Servicos;

public sealed class CalculadoraImpostoReforma : ICalculadoraImposto
{
    public decimal Calcular(decimal valorTotal) => valorTotal * TaxasImposto.TaxaReformaTributaria;
}
