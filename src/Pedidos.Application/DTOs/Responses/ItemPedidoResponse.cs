namespace Pedidos.Application.DTOs.Responses;

public sealed record ItemPedidoResponse(
    int ProdutoId,
    int Quantidade,
    decimal Valor
);
