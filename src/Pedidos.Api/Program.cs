using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Adiciona serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Pipeline HTTP
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
