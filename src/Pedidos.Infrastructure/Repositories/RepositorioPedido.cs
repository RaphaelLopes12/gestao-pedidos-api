using Microsoft.EntityFrameworkCore;
using Pedidos.Application.Interfaces;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Enums;
using Pedidos.Infrastructure.Persistence;

namespace Pedidos.Infrastructure.Repositories;

public sealed class RepositorioPedido : IRepositorioPedido
{
    private readonly PedidosDbContext _context;

    public RepositorioPedido(PedidosDbContext context)
    {
        _context = context;
    }

    public async Task<Pedido?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Pedido?> ObterPorPedidoExternoIdAsync(int pedidoExternoId, CancellationToken cancellationToken = default)
    {
        return await _context.Pedidos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PedidoExternoId == pedidoExternoId, cancellationToken);
    }

    public async Task<(List<Pedido> Pedidos, int Total)> ListarAsync(
        StatusPedido? status,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Pedidos
            .Include(p => p.Itens)
            .AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var pedidos = await query
            .OrderByDescending(p => p.CriadoEm)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        return (pedidos, total);
    }

    public async Task<int> AdicionarAsync(Pedido pedido, CancellationToken cancellationToken = default)
    {
        await _context.Pedidos.AddAsync(pedido, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return pedido.Id;
    }

    public async Task AtualizarAsync(Pedido pedido, CancellationToken cancellationToken = default)
    {
        _context.Pedidos.Update(pedido);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<HashSet<int>> ObterPedidoIdsExistentesAsync(
        IEnumerable<int> pedidoExternoIds,
        CancellationToken cancellationToken = default)
    {
        var idsList = pedidoExternoIds.ToList();

        if (idsList.Count == 0)
            return new HashSet<int>();

        var existentes = await _context.Pedidos
            .AsNoTracking()
            .Where(p => idsList.Contains(p.PedidoExternoId))
            .Select(p => p.PedidoExternoId)
            .ToListAsync(cancellationToken);

        return existentes.ToHashSet();
    }

    public async Task<List<int>> AdicionarEmLoteAsync(
        IEnumerable<Pedido> pedidos,
        CancellationToken cancellationToken = default)
    {
        var pedidosList = pedidos.ToList();

        if (pedidosList.Count == 0)
            return new List<int>();

        await _context.Pedidos.AddRangeAsync(pedidosList, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return pedidosList.Select(p => p.Id).ToList();
    }
}
