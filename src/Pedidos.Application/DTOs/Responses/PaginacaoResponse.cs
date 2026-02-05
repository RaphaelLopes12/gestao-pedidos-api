namespace Pedidos.Application.DTOs.Responses;

public sealed record PaginacaoResponse<T>(
    List<T> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalItens,
    int TotalPaginas
);
