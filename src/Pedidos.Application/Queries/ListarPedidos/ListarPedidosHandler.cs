using MediatR;
using Microsoft.Extensions.Logging;
using Pedidos.Application.DTOs.Responses;
using Pedidos.Application.Interfaces;

namespace Pedidos.Application.Queries.ListarPedidos;

public sealed class ListarPedidosHandler : IRequestHandler<ListarPedidosQuery, PaginacaoResponse<PedidoResponse>>
{
    private readonly IRepositorioPedido _repositorio;
    private readonly ILogger<ListarPedidosHandler> _logger;

    public ListarPedidosHandler(
        IRepositorioPedido repositorio,
        ILogger<ListarPedidosHandler> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task<PaginacaoResponse<PedidoResponse>> Handle(
        ListarPedidosQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Listando pedidos. Status: {Status}, Pagina: {Pagina}, TamanhoPagina: {TamanhoPagina}",
            query.Status?.ToString() ?? "Todos",
            query.Pagina,
            query.TamanhoPagina);

        var (pedidos, total) = await _repositorio.ListarAsync(
            query.Status,
            query.Pagina,
            query.TamanhoPagina,
            cancellationToken);

        var pedidosResponse = pedidos.Select(p => new PedidoResponse(
            p.Id,
            p.PedidoExternoId,
            p.ClienteId,
            p.Imposto,
            p.Itens.Select(i => new ItemPedidoResponse(
                i.ProdutoId,
                i.Quantidade,
                i.Valor)).ToList(),
            p.Status.ToString()
        )).ToList();

        var totalPaginas = (int)Math.Ceiling((double)total / query.TamanhoPagina);

        _logger.LogInformation(
            "Pedidos listados. Total: {Total}, TotalPaginas: {TotalPaginas}",
            total,
            totalPaginas);

        return new PaginacaoResponse<PedidoResponse>(
            pedidosResponse,
            query.Pagina,
            query.TamanhoPagina,
            total,
            totalPaginas);
    }
}
