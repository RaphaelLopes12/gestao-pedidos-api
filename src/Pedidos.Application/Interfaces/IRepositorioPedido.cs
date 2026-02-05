using Pedidos.Domain.Entities;
using Pedidos.Domain.Enums;

namespace Pedidos.Application.Interfaces;

public interface IRepositorioPedido
{
    Task<Pedido?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Pedido?> ObterPorPedidoExternoIdAsync(int pedidoExternoId, CancellationToken cancellationToken = default);
    Task<(List<Pedido> Pedidos, int Total)> ListarAsync(StatusPedido? status, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default);
    Task<int> AdicionarAsync(Pedido pedido, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Pedido pedido, CancellationToken cancellationToken = default);

    Task<HashSet<int>> ObterPedidoIdsExistentesAsync(IEnumerable<int> pedidoExternoIds, CancellationToken cancellationToken = default);
    Task<List<int>> AdicionarEmLoteAsync(IEnumerable<Pedido> pedidos, CancellationToken cancellationToken = default);
}
