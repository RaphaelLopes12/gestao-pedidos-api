namespace Pedidos.Application.DTOs.Responses;

public sealed record PedidoResponse(
    int Id,
    int PedidoExternoId,
    int ClienteId,
    string Status,
    decimal ValorTotal,
    decimal Imposto,
    DateTime CriadoEm,
    DateTime? ProcessadoEm,
    List<ItemPedidoResponse> Itens
);
