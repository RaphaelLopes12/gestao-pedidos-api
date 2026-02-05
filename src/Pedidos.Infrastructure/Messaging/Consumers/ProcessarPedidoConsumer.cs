using MassTransit;
using Microsoft.Extensions.Logging;
using Pedidos.Application.Events;
using Pedidos.Application.Interfaces;
using Pedidos.Domain.Enums;
using Pedidos.Domain.Services.Interfaces;

namespace Pedidos.Infrastructure.Messaging.Consumers;

/// <summary>
/// Consumer que processa pedidos em lote (fluxo assíncrono).
/// Recebe pedidos criados sem imposto, calcula o imposto e publica para Sistema B.
/// </summary>
public sealed class ProcessarPedidoConsumer : IConsumer<ProcessarPedidoLoteEvento>
{
    private readonly IRepositorioPedido _repositorio;
    private readonly ICalculadoraImposto _calculadoraImposto;
    private readonly IPublicadorEventos _publicador;
    private readonly ILogger<ProcessarPedidoConsumer> _logger;

    public ProcessarPedidoConsumer(
        IRepositorioPedido repositorio,
        ICalculadoraImposto calculadoraImposto,
        IPublicadorEventos publicador,
        ILogger<ProcessarPedidoConsumer> logger)
    {
        _repositorio = repositorio;
        _calculadoraImposto = calculadoraImposto;
        _publicador = publicador;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessarPedidoLoteEvento> context)
    {
        var pedidoId = context.Message.PedidoId;

        _logger.LogInformation(
            "Iniciando processamento assíncrono do pedido. PedidoId: {PedidoId}",
            pedidoId);

        try
        {
            var pedido = await _repositorio.ObterPorIdAsync(pedidoId, context.CancellationToken);

            if (pedido is null)
            {
                _logger.LogWarning("Pedido não encontrado para processamento. PedidoId: {PedidoId}", pedidoId);
                return;
            }

            if (pedido.Status != StatusPedido.Criado)
            {
                _logger.LogWarning(
                    "Pedido não está no status correto para processamento. PedidoId: {PedidoId}, Status: {Status}",
                    pedidoId,
                    pedido.Status);
                return;
            }

            // Calcula imposto
            var imposto = _calculadoraImposto.Calcular(pedido.ValorTotal);
            pedido.CalcularImposto(imposto);

            // Marca como processado
            pedido.MarcarComoProcessado();
            await _repositorio.AtualizarAsync(pedido, context.CancellationToken);

            _logger.LogInformation(
                "Pedido processado. PedidoId: {PedidoId}, Imposto: {Imposto}",
                pedidoId,
                imposto);

            // Publica para Sistema B
            var evento = new PedidoProcessadoEvento(
                pedido.Id,
                pedido.PedidoExternoId,
                pedido.ClienteId,
                pedido.ValorTotal,
                pedido.Imposto,
                pedido.ProcessadoEm!.Value);

            await _publicador.PublicarPedidoProcessadoAsync(evento, context.CancellationToken);

            _logger.LogInformation(
                "Pedido publicado para Sistema B. PedidoId: {PedidoId}",
                pedidoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Erro ao processar pedido. PedidoId: {PedidoId}",
                pedidoId);

            // Tenta marcar como erro
            try
            {
                var pedido = await _repositorio.ObterPorIdAsync(pedidoId, context.CancellationToken);
                if (pedido is not null && pedido.Status == StatusPedido.Criado)
                {
                    pedido.MarcarComoErro();
                    await _repositorio.AtualizarAsync(pedido, context.CancellationToken);
                }
            }
            catch
            {
                // Ignora erro ao marcar como erro
            }

            throw; // Re-throw para MassTransit fazer retry
        }
    }
}
