namespace Pedidos.Application.DTOs.Responses;

public sealed record CriarPedidoResponse(
    int Id,
    int PedidoExternoId,
    string Status,
    decimal ValorTotal,
    decimal Imposto,
    DateTime CriadoEm
);
