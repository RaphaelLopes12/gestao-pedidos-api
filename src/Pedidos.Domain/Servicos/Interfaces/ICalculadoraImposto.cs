namespace Pedidos.Domain.Servicos.Interfaces;

public interface ICalculadoraImposto
{
    decimal Calcular(decimal valorTotal);
}
