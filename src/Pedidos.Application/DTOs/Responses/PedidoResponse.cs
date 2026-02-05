namespace Pedidos.Application.DTOs.Responses;

public sealed record PedidoResponse(
    int Id,
    int PedidoId,
    int ClienteId,
    decimal Imposto,
    List<ItemPedidoResponse> Itens,
    string Status
);
