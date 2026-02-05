namespace Pedidos.Application.DTOs.Requests;

public sealed record CriarPedidoRequest(
    int PedidoId,
    int ClienteId,
    List<ItemPedidoRequest> Itens
);
