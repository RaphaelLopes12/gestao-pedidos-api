using MediatR;
using Pedidos.Application.DTOs.Responses;
using Pedidos.Domain.Enums;

namespace Pedidos.Application.Queries.ListarPedidos;

public sealed record ListarPedidosQuery(
    StatusPedido? Status,
    int Pagina,
    int TamanhoPagina
) : IRequest<PaginacaoResponse<PedidoResponse>>;
