using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pedidos.Infrastructure.Persistence;

public sealed class PedidosDbContextFactory : IDesignTimeDbContextFactory<PedidosDbContext>
{
    public PedidosDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PedidosDbContext>();

        // Connection string apenas para design-time (migrations)
        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=PedidosDb;User Id=sa;Password=SuaSenhaForte123!;TrustServerCertificate=True");

        return new PedidosDbContext(optionsBuilder.Options);
    }
}
