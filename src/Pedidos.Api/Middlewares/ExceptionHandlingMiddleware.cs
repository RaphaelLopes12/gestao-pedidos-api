using System.Net;
using System.Text.Json;
using FluentValidation;

namespace Pedidos.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                "Erro de validação",
                validationEx.Errors.Select(e => e.ErrorMessage).ToArray()),

            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                argEx.Message,
                Array.Empty<string>()),

            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "Recurso não encontrado",
                Array.Empty<string>()),

            _ => (
                HttpStatusCode.InternalServerError,
                "Ocorreu um erro interno no servidor",
                Array.Empty<string>())
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(
                exception,
                "Erro não tratado: {Message}",
                exception.Message);
        }
        else
        {
            _logger.LogWarning(
                "Erro tratado: {StatusCode} - {Message}",
                (int)statusCode,
                message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            status = (int)statusCode,
            message,
            errors = errors.Length > 0 ? errors : null
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, jsonOptions));
    }
}
