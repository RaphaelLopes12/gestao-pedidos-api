using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Pedidos.Application.DTOs.Responses;
using Pedidos.Application.Events;
using Pedidos.Application.Interfaces;
using Pedidos.Domain.Common;
using Pedidos.Domain.Entities;

namespace Pedidos.Application.Commands.CriarPedidosLote;

public sealed class CriarPedidosLoteHandler : IRequestHandler<CriarPedidosLoteCommand, Resultado<CriarLoteResponse>>
{
    private readonly IRepositorioPedido _repositorio;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CriarPedidosLoteHandler> _logger;

    public CriarPedidosLoteHandler(
        IRepositorioPedido repositorio,
        IPublishEndpoint publishEndpoint,
        ILogger<CriarPedidosLoteHandler> logger)
    {
        _repositorio = repositorio;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Resultado<CriarLoteResponse>> Handle(
        CriarPedidosLoteCommand command,
        CancellationToken cancellationToken)
    {
        var loteId = Guid.NewGuid();

        _logger.LogInformation(
            "Iniciando criação de lote. LoteId: {LoteId}, Quantidade: {Quantidade}",
            loteId,
            command.Pedidos.Count);

        var pedidosCriados = new List<int>();
        var erros = new List<string>();

        foreach (var pedidoRequest in command.Pedidos)
        {
            var pedidoExistente = await _repositorio.ObterPorPedidoExternoIdAsync(
                pedidoRequest.PedidoExternoId,
                cancellationToken);

            if (pedidoExistente is not null)
            {
                erros.Add($"PedidoExternoId {pedidoRequest.PedidoExternoId} já existe");
                continue;
            }

            var itensResultados = pedidoRequest.Itens
                .Select(i => ItemPedido.Criar(i.ProdutoId, i.Quantidade, i.ValorUnitario))
                .ToList();

            var itemComErro = itensResultados.FirstOrDefault(r => r.Falhou);
            if (itemComErro is not null)
            {
                erros.Add($"PedidoExternoId {pedidoRequest.PedidoExternoId}: {itemComErro.Erro}");
                continue;
            }

            var itens = itensResultados.Select(r => r.Valor!).ToList();

            var pedidoResultado = Pedido.Criar(
                pedidoRequest.PedidoExternoId,
                pedidoRequest.ClienteId,
                itens);

            if (pedidoResultado.Falhou)
            {
                erros.Add($"PedidoExternoId {pedidoRequest.PedidoExternoId}: {pedidoResultado.Erro}");
                continue;
            }

            var pedido = pedidoResultado.Valor!;
            var pedidoId = await _repositorio.AdicionarAsync(pedido, cancellationToken);
            pedidosCriados.Add(pedidoId);

            _logger.LogInformation(
                "Pedido do lote criado. LoteId: {LoteId}, PedidoId: {PedidoId}, PedidoExternoId: {PedidoExternoId}",
                loteId,
                pedidoId,
                pedidoRequest.PedidoExternoId);
        }

        foreach (var pedidoId in pedidosCriados)
        {
            await _publishEndpoint.Publish(
                new ProcessarPedidoLoteEvento(pedidoId),
                cancellationToken);
        }

        _logger.LogInformation(
            "Lote finalizado. LoteId: {LoteId}, Criados: {Criados}, Erros: {Erros}",
            loteId,
            pedidosCriados.Count,
            erros.Count);

        if (pedidosCriados.Count == 0)
        {
            return Resultado<CriarLoteResponse>.Falha(
                $"Nenhum pedido foi criado. Erros: {string.Join("; ", erros)}");
        }

        return Resultado<CriarLoteResponse>.Ok(new CriarLoteResponse(
            loteId,
            pedidosCriados.Count,
            "EmProcessamento",
            DateTime.UtcNow));
    }
}
