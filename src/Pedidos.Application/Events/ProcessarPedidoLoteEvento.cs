namespace Pedidos.Application.Events;

/// <summary>
/// Evento para processar pedido em lote (fluxo assíncrono).
/// Worker consome este evento para calcular imposto e publicar para Sistema B.
/// </summary>
public sealed record ProcessarPedidoLoteEvento(int PedidoId);
