using MediatR;
using Pedidos.Application.DTOs.Requests;
using Pedidos.Application.DTOs.Responses;
using Pedidos.Domain.Common;

namespace Pedidos.Application.Commands.CriarPedido;

public sealed record CriarPedidoCommand(
    int PedidoId,
    int ClienteId,
    List<ItemPedidoRequest> Itens
) : IRequest<Resultado<CriarPedidoResponse>>;
