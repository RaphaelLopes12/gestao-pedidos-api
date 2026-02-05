using MediatR;
using Pedidos.Application.DTOs.Requests;
using Pedidos.Application.DTOs.Responses;
using Pedidos.Domain.Common;

namespace Pedidos.Application.Commands.CriarPedidosLote;

public sealed record CriarPedidosLoteCommand(
    List<CriarPedidoRequest> Pedidos
) : IRequest<Resultado<CriarLoteResponse>>;
