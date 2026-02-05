namespace Pedidos.Application.DTOs.Responses;

public sealed record ItemPedidoResponse(
    int Id,
    int ProdutoId,
    int Quantidade,
    decimal ValorUnitario,
    decimal ValorTotal
);
