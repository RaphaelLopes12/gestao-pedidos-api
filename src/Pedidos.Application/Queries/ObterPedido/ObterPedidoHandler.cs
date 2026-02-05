using MediatR;
using Microsoft.Extensions.Logging;
using Pedidos.Application.DTOs.Responses;
using Pedidos.Application.Interfaces;
using Pedidos.Domain.Common;

namespace Pedidos.Application.Queries.ObterPedido;

public sealed class ObterPedidoHandler : IRequestHandler<ObterPedidoQuery, Resultado<PedidoResponse>>
{
    private readonly IRepositorioPedido _repositorio;
    private readonly ILogger<ObterPedidoHandler> _logger;

    public ObterPedidoHandler(
        IRepositorioPedido repositorio,
        ILogger<ObterPedidoHandler> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task<Resultado<PedidoResponse>> Handle(
        ObterPedidoQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando pedido. Id: {PedidoId}", query.PedidoId);

        var pedido = await _repositorio.ObterPorIdAsync(query.PedidoId, cancellationToken);

        if (pedido is null)
        {
            _logger.LogWarning("Pedido não encontrado. Id: {PedidoId}", query.PedidoId);
            return Resultado<PedidoResponse>.Falha("Pedido não encontrado");
        }

        var itensResponse = pedido.Itens
            .Select(i => new ItemPedidoResponse(
                i.Id,
                i.ProdutoId,
                i.Quantidade,
                i.ValorUnitario,
                i.ValorTotal))
            .ToList();

        var response = new PedidoResponse(
            pedido.Id,
            pedido.PedidoExternoId,
            pedido.ClienteId,
            pedido.Status.ToString(),
            pedido.ValorTotal,
            pedido.Imposto,
            pedido.CriadoEm,
            pedido.ProcessadoEm,
            itensResponse);

        _logger.LogInformation("Pedido encontrado. Id: {PedidoId}, Status: {Status}", query.PedidoId, pedido.Status);

        return Resultado<PedidoResponse>.Ok(response);
    }
}
