using MassTransit;
using Microsoft.Extensions.Logging;
using Pedidos.Application.Events;
using Pedidos.Application.Interfaces;

namespace Pedidos.Infrastructure.Messaging;

public sealed class PublicadorEventosRabbitMq : IPublicadorEventos
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PublicadorEventosRabbitMq> _logger;

    public PublicadorEventosRabbitMq(
        IPublishEndpoint publishEndpoint,
        ILogger<PublicadorEventosRabbitMq> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublicarPedidoProcessadoAsync(
        PedidoProcessadoEvento evento,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Publicando evento PedidoProcessado. PedidoId: {PedidoId}, PedidoExternoId: {PedidoExternoId}",
            evento.PedidoId,
            evento.PedidoExternoId);

        await _publishEndpoint.Publish(evento, cancellationToken);

        _logger.LogInformation(
            "Evento PedidoProcessado publicado com sucesso. PedidoId: {PedidoId}",
            evento.PedidoId);
    }
}
