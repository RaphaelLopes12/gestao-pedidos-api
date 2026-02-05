using Pedidos.Domain.Common;

namespace Pedidos.Domain.Entities;

public sealed class ItemPedido
{
    public int Id { get; private set; }
    public int PedidoId { get; private set; }
    public int ProdutoId { get; private set; }
    public int Quantidade { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public decimal ValorTotal => Quantidade * ValorUnitario;

    private ItemPedido() { }

    private ItemPedido(int produtoId, int quantidade, decimal valorUnitario)
    {
        ProdutoId = produtoId;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
    }

    public static Resultado<ItemPedido> Criar(int produtoId, int quantidade, decimal valorUnitario)
    {
        if (produtoId <= 0)
            return Resultado<ItemPedido>.Falha("ProdutoId deve ser maior que zero");

        if (quantidade <= 0)
            return Resultado<ItemPedido>.Falha("Quantidade deve ser maior que zero");

        if (valorUnitario <= 0)
            return Resultado<ItemPedido>.Falha("Valor unitário deve ser maior que zero");

        return Resultado<ItemPedido>.Ok(new ItemPedido(produtoId, quantidade, valorUnitario));
    }
}
