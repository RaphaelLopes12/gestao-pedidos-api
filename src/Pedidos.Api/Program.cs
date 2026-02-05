using Asp.Versioning;
using FluentValidation;
using Microsoft.OpenApi.Models;
using Pedidos.Api.Middlewares;
using Pedidos.Api.Validators;
using Pedidos.Application.Commands.CriarPedido;
using Pedidos.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CriarPedidoCommand).Assembly));

builder.Services.AddValidatorsFromAssemblyContaining<CriarPedidoCommandValidator>();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"));
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gestão de Pedidos API",
        Version = "v1",
        Description = "API para gerenciamento de pedidos com cálculo de impostos",
        Contact = new OpenApiContact
        {
            Name = "Raphael Ferreira Lopes",
            Email = "Raphaellopes228@gmail.com"
        }
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Gestão de Pedidos API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
