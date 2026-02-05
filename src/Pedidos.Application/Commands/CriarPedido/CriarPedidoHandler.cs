using MediatR;
using Microsoft.Extensions.Logging;
using Pedidos.Application.DTOs.Responses;
using Pedidos.Application.Events;
using Pedidos.Application.Interfaces;
using Pedidos.Domain.Common;
using Pedidos.Domain.Entities;
using Pedidos.Domain.Services.Interfaces;

namespace Pedidos.Application.Commands.CriarPedido;

public sealed class CriarPedidoHandler : IRequestHandler<CriarPedidoCommand, Resultado<CriarPedidoResponse>>
{
    private readonly IRepositorioPedido _repositorio;
    private readonly ICalculadoraImposto _calculadoraImposto;
    private readonly IPublicadorEventos _publicador;
    private readonly ILogger<CriarPedidoHandler> _logger;

    public CriarPedidoHandler(
        IRepositorioPedido repositorio,
        ICalculadoraImposto calculadoraImposto,
        IPublicadorEventos publicador,
        ILogger<CriarPedidoHandler> logger)
    {
        _repositorio = repositorio;
        _calculadoraImposto = calculadoraImposto;
        _publicador = publicador;
        _logger = logger;
    }

    public async Task<Resultado<CriarPedidoResponse>> Handle(
        CriarPedidoCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Iniciando criação de pedido. PedidoId: {PedidoId}, ClienteId: {ClienteId}",
            command.PedidoId,
            command.ClienteId);

        var pedidoExistente = await _repositorio.ObterPorPedidoExternoIdAsync(
            command.PedidoId,
            cancellationToken);

        if (pedidoExistente is not null)
        {
            _logger.LogWarning(
                "Pedido duplicado. PedidoId: {PedidoId} já existe",
                command.PedidoId);
            return Resultado<CriarPedidoResponse>.Falha("Pedido já existe com este PedidoId");
        }

        var itensResultados = command.Itens
            .Select(i => ItemPedido.Criar(i.ProdutoId, i.Quantidade, i.Valor))
            .ToList();

        var itemComErro = itensResultados.FirstOrDefault(r => r.Falhou);
        if (itemComErro is not null)
        {
            _logger.LogWarning("Erro ao criar item do pedido: {Erro}", itemComErro.Erro);
            return Resultado<CriarPedidoResponse>.Falha(itemComErro.Erro);
        }

        var itens = itensResultados.Select(r => r.Valor!).ToList();

        var pedidoResultado = Pedido.Criar(command.PedidoId, command.ClienteId, itens);

        if (pedidoResultado.Falhou)
        {
            _logger.LogWarning("Erro ao criar pedido: {Erro}", pedidoResultado.Erro);
            return Resultado<CriarPedidoResponse>.Falha(pedidoResultado.Erro);
        }

        var pedido = pedidoResultado.Valor!;

        var imposto = _calculadoraImposto.Calcular(pedido.ValorTotal);
        pedido.CalcularImposto(imposto);

        var pedidoId = await _repositorio.AdicionarAsync(pedido, cancellationToken);

        _logger.LogInformation(
            "Pedido criado com sucesso. Id: {Id}, Imposto: {Imposto}",
            pedidoId,
            imposto);

        pedido.MarcarComoProcessado();
        await _repositorio.AtualizarAsync(pedido, cancellationToken);

        var evento = new PedidoProcessadoEvento(
            pedidoId,
            pedido.PedidoExternoId,
            pedido.ClienteId,
            pedido.ValorTotal,
            pedido.Imposto,
            pedido.ProcessadoEm!.Value);

        try
        {
            await _publicador.PublicarPedidoProcessadoAsync(evento, cancellationToken);

            _logger.LogInformation(
                "Pedido processado e publicado para Sistema B. Id: {Id}",
                pedidoId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Falha ao publicar evento para Sistema B. O pedido foi criado. Id: {Id}",
                pedidoId);
        }

        return Resultado<CriarPedidoResponse>.Ok(new CriarPedidoResponse(
            pedidoId,
            pedido.Status.ToString()));
    }
}
