using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pedidos.Domain.Entities;

namespace Pedidos.Infrastructure.Persistence.Configurations;

public sealed class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.PedidoExternoId)
            .IsRequired();

        builder.Property(p => p.ClienteId)
            .IsRequired();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Imposto)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.CriadoEm)
            .IsRequired();

        builder.Property(p => p.ProcessadoEm);

        builder.HasIndex(p => p.PedidoExternoId)
            .IsUnique()
            .HasDatabaseName("IX_Pedidos_PedidoExternoId");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Pedidos_Status");

        builder.HasIndex(p => p.CriadoEm)
            .HasDatabaseName("IX_Pedidos_CriadoEm");

        builder.HasIndex(p => new { p.Status, p.CriadoEm })
            .HasDatabaseName("IX_Pedidos_Status_CriadoEm");

        builder.HasMany(p => p.Itens)
            .WithOne()
            .HasForeignKey(i => i.PedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(p => p.ValorTotal);
    }
}
