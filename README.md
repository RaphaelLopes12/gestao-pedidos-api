# API de Gestão de Pedidos

API REST para gerenciamento de pedidos com cálculo de impostos e integração via mensageria.

## Tecnologias Utilizadas

- **.NET 8** (LTS)
- **Entity Framework Core** - ORM
- **SQL Server** - Banco de dados
- **MassTransit + RabbitMQ** - Mensageria
- **MediatR** - CQRS (Commands/Queries)
- **FluentValidation** - Validação de requests
- **Serilog** - Logging estruturado
- **xUnit + FluentAssertions + NSubstitute + Bogus** - Testes
- **Testcontainers** - Testes de integração

## Arquitetura

```
┌─────────────────────────────────────────────────────────────────────┐
│                           Pedidos.Api                                │
│  Controllers, Middlewares, Validators                                │
└─────────────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────────────┐
│                        Pedidos.Application                           │
│  Commands, Queries, DTOs, Events, Interfaces                         │
│  (CQRS com MediatR)                                                  │
└─────────────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────────────┐
│                          Pedidos.Domain                              │
│  Entities, Enums, Services (Calculadora Imposto)                     │
└─────────────────────────────────────────────────────────────────────┘
                                │
┌─────────────────────────────────────────────────────────────────────┐
│                       Pedidos.Infrastructure                         │
│  DbContext, Repositories, Messaging (RabbitMQ)                       │
└─────────────────────────────────────────────────────────────────────┘
```

## Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/products/docker-desktop) e Docker Compose

## Executando com Docker

```bash
# Clonar o repositório
git clone <url-do-repositorio>
cd gestao-pedidos-api

# Subir todos os serviços (API + SQL Server + RabbitMQ)
docker-compose up -d

# Verificar logs da API
docker-compose logs -f api
```

A API estará disponível em: `http://localhost:5000`

### Serviços

| Serviço | Porta | Descrição |
|---------|-------|-----------|
| API | 5000 | API REST |
| SQL Server | 1433 | Banco de dados |
| RabbitMQ | 5672 | Mensageria (AMQP) |
| RabbitMQ Management | 15672 | Interface web (guest/guest) |

## Executando Localmente (Desenvolvimento)

```bash
# 1. Subir dependências (SQL Server + RabbitMQ)
docker-compose up -d sqlserver rabbitmq

# 2. Restaurar pacotes
dotnet restore

# 3. Aplicar migrations
dotnet ef database update --project src/Pedidos.Infrastructure --startup-project src/Pedidos.Api

# 4. Executar a API
dotnet run --project src/Pedidos.Api
```

## Endpoints

### Pedidos

| Método | Endpoint | Descrição | Status |
|--------|----------|-----------|--------|
| POST | `/api/v1/pedidos` | Criar pedido (síncrono) | 201 Created |
| POST | `/api/v1/pedidos/lote` | Criar lote de pedidos (assíncrono) | 202 Accepted |
| GET | `/api/v1/pedidos/{id}` | Obter pedido por ID | 200 OK / 404 |
| GET | `/api/v1/pedidos` | Listar pedidos (paginado) | 200 OK |

### Health Check

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/health` | Status da aplicação |

## Exemplos de Uso

### Criar Pedido

```bash
curl -X POST http://localhost:5000/api/v1/pedidos \
  -H "Content-Type: application/json" \
  -d '{
    "pedidoExternoId": 12345,
    "clienteId": 100,
    "itens": [
      {
        "produtoId": 1,
        "quantidade": 2,
        "valorUnitario": 50.00
      }
    ]
  }'
```

**Resposta (201 Created):**
```json
{
  "id": 1,
  "pedidoExternoId": 12345,
  "status": "Processado",
  "valorTotal": 100.00,
  "imposto": 30.00,
  "criadoEm": "2026-02-05T12:00:00Z"
}
```

### Criar Lote de Pedidos

```bash
curl -X POST http://localhost:5000/api/v1/pedidos/lote \
  -H "Content-Type: application/json" \
  -d '{
    "pedidos": [
      {
        "pedidoExternoId": 20001,
        "clienteId": 100,
        "itens": [{ "produtoId": 1, "quantidade": 1, "valorUnitario": 100.00 }]
      },
      {
        "pedidoExternoId": 20002,
        "clienteId": 101,
        "itens": [{ "produtoId": 2, "quantidade": 2, "valorUnitario": 50.00 }]
      }
    ]
  }'
```

**Resposta (202 Accepted):**
```json
{
  "loteId": "550e8400-e29b-41d4-a716-446655440000",
  "quantidadePedidos": 2,
  "status": "EmProcessamento"
}
```

### Listar Pedidos

```bash
curl "http://localhost:5000/api/v1/pedidos?pagina=1&tamanhoPagina=10&status=Processado"
```

**Resposta (200 OK):**
```json
{
  "itens": [
    {
      "id": 1,
      "pedidoExternoId": 12345,
      "clienteId": 100,
      "status": "Processado",
      "valorTotal": 100.00,
      "imposto": 30.00,
      "criadoEm": "2026-02-05T12:00:00Z"
    }
  ],
  "pagina": 1,
  "tamanhoPagina": 10,
  "totalItens": 1,
  "totalPaginas": 1
}
```

## Feature Flag - Cálculo de Imposto

A API suporta duas formas de cálculo de imposto, controladas via configuração:

| Modo | Taxa | Configuração |
|------|------|--------------|
| Atual | 30% | `FeatureFlags:UsarReformaTributaria = false` |
| Reforma Tributária | 20% | `FeatureFlags:UsarReformaTributaria = true` |

### Configuração via appsettings.json

```json
{
  "FeatureFlags": {
    "UsarReformaTributaria": false
  }
}
```

### Configuração via variável de ambiente

```bash
FeatureFlags__UsarReformaTributaria=true
```

## Integração com Sistema B

Os pedidos processados são disponibilizados para o Sistema B via mensageria (RabbitMQ).

Após o processamento, um evento `PedidoProcessadoEvento` é publicado na fila:

```json
{
  "pedidoId": 1,
  "pedidoExternoId": 12345,
  "clienteId": 100,
  "valorTotal": 100.00,
  "imposto": 30.00,
  "status": "Processado",
  "processadoEm": "2026-02-05T12:00:00Z"
}
```

O Sistema B deve consumir esta fila para receber os pedidos.

## Testes

### Executar Todos os Testes

```bash
dotnet test
```

### Executar Testes Unitários

```bash
dotnet test tests/Pedidos.UnitTests
```

### Executar Testes de Integração

```bash
# Requer Docker rodando (Testcontainers)
dotnet test tests/Pedidos.IntegrationTests
```

### Cobertura de Testes

| Tipo | Quantidade | Descrição |
|------|------------|-----------|
| Unitários | 64 | Domain + Application |
| Integração | 13 | API + Testcontainers |
| **Total** | **77** | |

## Estrutura do Projeto

```
gestao-pedidos-api/
├── src/
│   ├── Pedidos.Api/          # Controllers, Middlewares, Validators
│   ├── Pedidos.Application/  # Commands, Queries, DTOs, Events
│   ├── Pedidos.Domain/       # Entities, Enums, Services
│   └── Pedidos.Infrastructure/ # DbContext, Repositories, Messaging
├── tests/
│   ├── Pedidos.UnitTests/        # Testes unitários
│   └── Pedidos.IntegrationTests/ # Testes de integração
├── docker-compose.yml
└── README.md
```

## Fluxo de Processamento

### Pedido Unitário (Síncrono)

```
Sistema A                    API                         RabbitMQ
    │                         │                              │
    │  POST /pedidos          │                              │
    ├────────────────────────►│                              │
    │                         │ 1. Valida request            │
    │                         │ 2. Verifica duplicidade      │
    │                         │ 3. Calcula imposto           │
    │                         │ 4. Persiste pedido           │
    │                         │ 5. Publica evento ──────────►│
    │                         │ 6. Retorna resposta          │
    │◄────────────────────────┤                              │
    │  201 Created            │                              │
```

### Lote de Pedidos (Assíncrono)

```
Sistema A                    API                    Background Worker
    │                         │                              │
    │  POST /pedidos/lote     │                              │
    ├────────────────────────►│                              │
    │                         │ 1. Valida estrutura          │
    │                         │ 2. Enfileira para processo   │
    │◄────────────────────────┤                              │
    │  202 Accepted           │                              │
    │                         │         ┌────────────────────┤
    │                         │         │ Worker processa    │
    │                         │         │ cada pedido        │
    │                         │         └────────────────────┤
```

## Variáveis de Ambiente

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `ConnectionStrings__DefaultConnection` | Connection string SQL Server | - |
| `RabbitMq__Host` | Host do RabbitMQ | localhost |
| `RabbitMq__Username` | Usuário RabbitMQ | guest |
| `RabbitMq__Password` | Senha RabbitMQ | guest |
| `FeatureFlags__UsarReformaTributaria` | Usar cálculo reforma tributária | false |

## Observabilidade

A aplicação utiliza **Serilog** para logging estruturado com as seguintes features:

- Logs em formato JSON
- Correlation ID por request
- Enriquecimento com informações de contexto
- Saída para console e arquivo

### Exemplo de Log

```json
{
  "Timestamp": "2026-02-05T12:00:00.000Z",
  "Level": "Information",
  "Message": "Pedido criado com sucesso",
  "Properties": {
    "PedidoId": 1,
    "PedidoExternoId": 12345,
    "RequestId": "abc-123",
    "SourceContext": "Pedidos.Application.Commands.CriarPedido.CriarPedidoHandler"
  }
}
```

## Licença

Este projeto foi desenvolvido como teste técnico.
