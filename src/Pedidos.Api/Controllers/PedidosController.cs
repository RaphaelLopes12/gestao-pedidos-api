using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pedidos.Application.Commands.CriarPedido;
using Pedidos.Application.Commands.CriarPedidosLote;
using Pedidos.Application.DTOs.Requests;
using Pedidos.Application.DTOs.Responses;
using Pedidos.Application.Queries.ListarPedidos;
using Pedidos.Application.Queries.ObterPedido;
using Pedidos.Domain.Enums;

namespace Pedidos.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public sealed class PedidosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CriarPedidoCommand> _validator;
    private readonly IValidator<CriarPedidosLoteCommand> _validatorLote;
    private readonly ILogger<PedidosController> _logger;

    public PedidosController(
        IMediator mediator,
        IValidator<CriarPedidoCommand> validator,
        IValidator<CriarPedidosLoteCommand> validatorLote,
        ILogger<PedidosController> logger)
    {
        _mediator = mediator;
        _validator = validator;
        _validatorLote = validatorLote;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo pedido (fluxo síncrono)
    /// </summary>
    /// <param name="request">Dados do pedido</param>
    /// <returns>Pedido criado com imposto calculado</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CriarPedidoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarPedidoRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Recebida requisição para criar pedido. PedidoId: {PedidoId}",
            request.PedidoId);

        var command = new CriarPedidoCommand(
            request.PedidoId,
            request.ClienteId,
            request.Itens);

        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Erro de validação",
                errors = validationResult.Errors.Select(e => e.ErrorMessage)
            });
        }

        var resultado = await _mediator.Send(command, cancellationToken);

        if (resultado.Falhou)
        {
            if (resultado.Erro.Contains("já existe"))
            {
                return Conflict(new { status = 409, message = resultado.Erro });
            }
            return BadRequest(new { status = 400, message = resultado.Erro });
        }

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = resultado.Valor!.Id },
            resultado.Valor);
    }

    /// <summary>
    /// Cria pedidos em lote (fluxo assíncrono)
    /// </summary>
    /// <param name="request">Lista de pedidos</param>
    /// <returns>Informações do lote em processamento</returns>
    [HttpPost("lote")]
    [ProducesResponseType(typeof(CriarLoteResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CriarLote(
        [FromBody] CriarPedidosLoteRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Recebida requisição para criar lote de pedidos. Quantidade: {Quantidade}",
            request.Pedidos?.Count ?? 0);

        var command = new CriarPedidosLoteCommand(request.Pedidos ?? []);

        var validationResult = await _validatorLote.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                status = 400,
                message = "Erro de validação",
                errors = validationResult.Errors.Select(e => e.ErrorMessage)
            });
        }

        var resultado = await _mediator.Send(command, cancellationToken);

        if (resultado.Falhou)
        {
            return BadRequest(new { status = 400, message = resultado.Erro });
        }

        return Accepted(resultado.Valor);
    }

    /// <summary>
    /// Obtém um pedido por ID
    /// </summary>
    /// <param name="id">ID do pedido</param>
    /// <returns>Dados do pedido</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recebida requisição para obter pedido. Id: {Id}", id);

        var query = new ObterPedidoQuery(id);
        var resultado = await _mediator.Send(query, cancellationToken);

        if (resultado.Falhou)
        {
            return NotFound(new { status = 404, message = resultado.Erro });
        }

        return Ok(resultado.Valor);
    }

    /// <summary>
    /// Lista pedidos com paginação
    /// </summary>
    /// <param name="status">Filtro por status (opcional)</param>
    /// <param name="pagina">Número da página (padrão: 1)</param>
    /// <param name="tamanhoPagina">Tamanho da página (padrão: 20, máximo: 100)</param>
    /// <returns>Lista paginada de pedidos</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginacaoResponse<PedidoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] StatusPedido? status,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Recebida requisição para listar pedidos. Status: {Status}, Pagina: {Pagina}",
            status?.ToString() ?? "Todos",
            pagina);

        if (pagina < 1) pagina = 1;
        if (tamanhoPagina < 1) tamanhoPagina = 20;
        if (tamanhoPagina > 100) tamanhoPagina = 100;

        var query = new ListarPedidosQuery(status, pagina, tamanhoPagina);
        var resultado = await _mediator.Send(query, cancellationToken);

        return Ok(resultado);
    }
}
