namespace Pedidos.Domain.Common;

public class Resultado
{
    public bool Sucesso { get; }
    public string Erro { get; }
    public bool Falhou => !Sucesso;

    protected Resultado(bool sucesso, string erro)
    {
        Sucesso = sucesso;
        Erro = erro;
    }

    public static Resultado Ok() => new(true, string.Empty);
    public static Resultado Falha(string erro) => new(false, erro);
}

public class Resultado<T> : Resultado
{
    public T? Valor { get; }

    private Resultado(bool sucesso, T? valor, string erro)
        : base(sucesso, erro)
    {
        Valor = valor;
    }

    public static Resultado<T> Ok(T valor) => new(true, valor, string.Empty);
    public static new Resultado<T> Falha(string erro) => new(false, default, erro);
}
