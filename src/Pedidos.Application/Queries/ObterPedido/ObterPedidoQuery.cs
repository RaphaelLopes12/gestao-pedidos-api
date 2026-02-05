using MediatR;
using Pedidos.Application.DTOs.Responses;
using Pedidos.Domain.Common;

namespace Pedidos.Application.Queries.ObterPedido;

public sealed record ObterPedidoQuery(int PedidoId) : IRequest<Resultado<PedidoResponse>>;
