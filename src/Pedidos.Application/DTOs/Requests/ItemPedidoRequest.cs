namespace Pedidos.Application.DTOs.Requests;

public sealed record ItemPedidoRequest(
    int ProdutoId,
    int Quantidade,
    decimal Valor
);
