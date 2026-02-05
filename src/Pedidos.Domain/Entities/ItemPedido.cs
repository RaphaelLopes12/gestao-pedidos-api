using Pedidos.Domain.Common;

namespace Pedidos.Domain.Entities;

public sealed class ItemPedido
{
    public int Id { get; private set; }
    public int PedidoId { get; private set; }
    public int ProdutoId { get; private set; }
    public int Quantidade { get; private set; }
    public decimal Valor { get; private set; }

    private ItemPedido() { }

    private ItemPedido(int produtoId, int quantidade, decimal valor)
    {
        ProdutoId = produtoId;
        Quantidade = quantidade;
        Valor = valor;
    }

    public static Resultado<ItemPedido> Criar(int produtoId, int quantidade, decimal valor)
    {
        if (produtoId <= 0)
            return Resultado<ItemPedido>.Falha("ProdutoId deve ser maior que zero");

        if (quantidade <= 0)
            return Resultado<ItemPedido>.Falha("Quantidade deve ser maior que zero");

        if (valor <= 0)
            return Resultado<ItemPedido>.Falha("Valor deve ser maior que zero");

        return Resultado<ItemPedido>.Ok(new ItemPedido(produtoId, quantidade, valor));
    }
}
