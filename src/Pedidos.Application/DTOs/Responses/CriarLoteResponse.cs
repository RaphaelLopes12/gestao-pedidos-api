namespace Pedidos.Application.DTOs.Responses;

public sealed record CriarLoteResponse(
    Guid LoteId,
    int QuantidadePedidos,
    string Status,
    DateTime RecebidoEm
);
