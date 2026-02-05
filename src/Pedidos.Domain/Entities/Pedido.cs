using Pedidos.Domain.Common;
using Pedidos.Domain.Enums;

namespace Pedidos.Domain.Entities;

public sealed class Pedido
{
    public int Id { get; private set; }
    public int PedidoExternoId { get; private set; }
    public int ClienteId { get; private set; }
    public StatusPedido Status { get; private set; }
    public decimal Imposto { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? ProcessadoEm { get; private set; }

    private readonly List<ItemPedido> _itens = [];
    public IReadOnlyCollection<ItemPedido> Itens => _itens.AsReadOnly();

    public decimal ValorTotal => _itens.Sum(i => i.Valor);

    private Pedido() { }

    private Pedido(int pedidoExternoId, int clienteId, List<ItemPedido> itens)
    {
        PedidoExternoId = pedidoExternoId;
        ClienteId = clienteId;
        Status = StatusPedido.Criado;
        CriadoEm = DateTime.UtcNow;
        _itens = itens;
    }

    public static Resultado<Pedido> Criar(int pedidoExternoId, int clienteId, List<ItemPedido> itens)
    {
        if (pedidoExternoId <= 0)
            return Resultado<Pedido>.Falha("PedidoExternoId deve ser maior que zero");

        if (clienteId <= 0)
            return Resultado<Pedido>.Falha("ClienteId deve ser maior que zero");

        if (itens is null || itens.Count == 0)
            return Resultado<Pedido>.Falha("Pedido deve ter pelo menos um item");

        return Resultado<Pedido>.Ok(new Pedido(pedidoExternoId, clienteId, itens));
    }

    public void CalcularImposto(decimal valorImposto)
    {
        Imposto = valorImposto;
    }

    public void MarcarComoProcessado()
    {
        Status = StatusPedido.Processado;
        ProcessadoEm = DateTime.UtcNow;
    }

    public void MarcarComoEnviado()
    {
        Status = StatusPedido.Enviado;
    }

    public void MarcarComoErro()
    {
        Status = StatusPedido.Erro;
    }
}
