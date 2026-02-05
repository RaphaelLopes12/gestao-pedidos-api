namespace Pedidos.Domain.Services.Interfaces;

public interface ICalculadoraImposto
{
    decimal Calcular(decimal valorTotal);
}
