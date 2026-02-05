using MediatR;
using Pedidos.Domain.Common;

namespace Pedidos.Application.Commands.ConfirmarEnvio;

public sealed record ConfirmarEnvioCommand(int PedidoId) : IRequest<Resultado>;
