using System.Diagnostics;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Pedidos.Application.DTOs.Requests;
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
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Iniciando criação de lote. LoteId: {LoteId}, Quantidade: {Quantidade}",
            loteId,
            command.Pedidos.Count);

        var pedidoIdsNoRequest = command.Pedidos.Select(p => p.PedidoId).ToList();
        var idsExistentes = await _repositorio.ObterPedidoIdsExistentesAsync(pedidoIdsNoRequest, cancellationToken);

        _logger.LogInformation(
            "Verificação de duplicidade concluída. LoteId: {LoteId}, Existentes: {Existentes}, Tempo: {Tempo}ms",
            loteId,
            idsExistentes.Count,
            stopwatch.ElapsedMilliseconds);

        var pedidosParaInserir = new List<Pedido>();
        var erros = new List<string>();

        foreach (var pedidoRequest in command.Pedidos)
        {
            if (idsExistentes.Contains(pedidoRequest.PedidoId))
            {
                erros.Add($"PedidoId {pedidoRequest.PedidoId} já existe");
                continue;
            }

            var pedidoOuErro = CriarPedidoEntidade(pedidoRequest);
            if (pedidoOuErro.Falhou)
            {
                erros.Add($"PedidoId {pedidoRequest.PedidoId}: {pedidoOuErro.Erro}");
                continue;
            }

            pedidosParaInserir.Add(pedidoOuErro.Valor!);
        }

        if (pedidosParaInserir.Count == 0)
        {
            return Resultado<CriarLoteResponse>.Falha(
                $"Nenhum pedido foi criado. Erros: {string.Join("; ", erros)}");
        }

        var pedidoIds = await _repositorio.AdicionarEmLoteAsync(pedidosParaInserir, cancellationToken);

        await PublicarEventosAsync(pedidoIds, loteId, cancellationToken);

        stopwatch.Stop();
        _logger.LogInformation(
            "Lote finalizado. LoteId: {LoteId}, Criados: {Criados}, Erros: {Erros}, TempoTotal: {Tempo}ms",
            loteId,
            pedidoIds.Count,
            erros.Count,
            stopwatch.ElapsedMilliseconds);

        return Resultado<CriarLoteResponse>.Ok(new CriarLoteResponse(
            loteId,
            pedidoIds.Count,
            "EmProcessamento",
            DateTime.UtcNow));
    }

    private static Resultado<Pedido> CriarPedidoEntidade(CriarPedidoRequest pedidoRequest)
    {
        var itensResultados = pedidoRequest.Itens
            .Select(i => ItemPedido.Criar(i.ProdutoId, i.Quantidade, i.Valor))
            .ToList();

        var itemComErro = itensResultados.FirstOrDefault(r => r.Falhou);
        if (itemComErro is not null)
            return Resultado<Pedido>.Falha(itemComErro.Erro!);

        var itens = itensResultados.Select(r => r.Valor!).ToList();

        return Pedido.Criar(pedidoRequest.PedidoId, pedidoRequest.ClienteId, itens);
    }

    private async Task PublicarEventosAsync(List<int> pedidoIds, Guid loteId, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var pedidoId in pedidoIds)
            {
                await _publishEndpoint.Publish(new ProcessarPedidoLoteEvento(pedidoId), cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Falha ao publicar eventos para processamento. Os pedidos foram criados. LoteId: {LoteId}",
                loteId);
        }
    }
}
