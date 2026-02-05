using MediatR;
using Microsoft.Extensions.Logging;
using Pedidos.Application.Interfaces;
using Pedidos.Domain.Common;
using Pedidos.Domain.Enums;

namespace Pedidos.Application.Commands.ConfirmarEnvio;

public sealed class ConfirmarEnvioHandler : IRequestHandler<ConfirmarEnvioCommand, Resultado>
{
    private readonly IRepositorioPedido _repositorio;
    private readonly ILogger<ConfirmarEnvioHandler> _logger;

    public ConfirmarEnvioHandler(
        IRepositorioPedido repositorio,
        ILogger<ConfirmarEnvioHandler> logger)
    {
        _repositorio = repositorio;
        _logger = logger;
    }

    public async Task<Resultado> Handle(
        ConfirmarEnvioCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Sistema B confirmando recebimento do pedido. PedidoId: {PedidoId}",
            command.PedidoId);

        var pedido = await _repositorio.ObterPorIdAsync(command.PedidoId, cancellationToken);

        if (pedido is null)
        {
            _logger.LogWarning("Pedido não encontrado. Id: {PedidoId}", command.PedidoId);
            return Resultado.Falha("Pedido não encontrado");
        }

        if (pedido.Status != StatusPedido.Processado)
        {
            _logger.LogWarning(
                "Pedido não está no status correto para confirmação. Id: {PedidoId}, Status: {Status}",
                command.PedidoId,
                pedido.Status);
            return Resultado.Falha($"Pedido não pode ser confirmado. Status atual: {pedido.Status}");
        }

        pedido.MarcarComoEnviado();
        await _repositorio.AtualizarAsync(pedido, cancellationToken);

        _logger.LogInformation(
            "Pedido confirmado como enviado para Sistema B. PedidoId: {PedidoId}",
            command.PedidoId);

        return Resultado.Ok();
    }
}
