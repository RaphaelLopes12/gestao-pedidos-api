using Microsoft.EntityFrameworkCore;
using Pedidos.Domain.Entities;

namespace Pedidos.Infrastructure.Persistence;

public sealed class PedidosDbContext : DbContext
{
    public PedidosDbContext(DbContextOptions<PedidosDbContext> options) : base(options)
    {
    }

    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PedidosDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
