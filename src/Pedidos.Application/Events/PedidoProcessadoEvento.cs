namespace Pedidos.Application.Events;

public sealed record PedidoProcessadoEvento(
    int PedidoId,
    int PedidoExternoId,
    int ClienteId,
    decimal ValorTotal,
    decimal Imposto,
    DateTime ProcessadoEm
);
