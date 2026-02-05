using Pedidos.Application.Events;

namespace Pedidos.Application.Interfaces;

public interface IPublicadorEventos
{
    Task PublicarPedidoProcessadoAsync(PedidoProcessadoEvento evento, CancellationToken cancellationToken = default);
}
