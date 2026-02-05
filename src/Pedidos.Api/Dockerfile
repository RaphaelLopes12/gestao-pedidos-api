# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia arquivos de projeto para restore
COPY ["src/Pedidos.Api/Pedidos.Api.csproj", "src/Pedidos.Api/"]
COPY ["src/Pedidos.Application/Pedidos.Application.csproj", "src/Pedidos.Application/"]
COPY ["src/Pedidos.Domain/Pedidos.Domain.csproj", "src/Pedidos.Domain/"]
COPY ["src/Pedidos.Infrastructure/Pedidos.Infrastructure.csproj", "src/Pedidos.Infrastructure/"]

# Restore de dependencias
RUN dotnet restore "src/Pedidos.Api/Pedidos.Api.csproj"

# Copia todo o codigo fonte
COPY . .

# Build da aplicacao
WORKDIR "/src/src/Pedidos.Api"
RUN dotnet build "Pedidos.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Pedidos.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Cria usuario nao-root para seguranca
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copia arquivos publicados
COPY --from=publish /app/publish .

# Variaveis de ambiente
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Exposicao de porta
EXPOSE 8080

# Health check (usando wget que esta disponivel na imagem)
HEALTHCHECK --interval=30s --timeout=10s --start-period=10s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Entrypoint
ENTRYPOINT ["dotnet", "Pedidos.Api.dll"]
