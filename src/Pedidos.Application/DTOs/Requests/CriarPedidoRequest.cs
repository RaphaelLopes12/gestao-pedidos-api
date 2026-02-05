namespace Pedidos.Application.DTOs.Requests;

public sealed record CriarPedidoRequest(
    int PedidoExternoId,
    int ClienteId,
    List<ItemPedidoRequest> Itens
);
