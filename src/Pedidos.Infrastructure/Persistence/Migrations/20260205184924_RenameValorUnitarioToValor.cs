using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pedidos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameValorUnitarioToValor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ValorUnitario",
                table: "ItensPedido",
                newName: "Valor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Valor",
                table: "ItensPedido",
                newName: "ValorUnitario");
        }
    }
}
