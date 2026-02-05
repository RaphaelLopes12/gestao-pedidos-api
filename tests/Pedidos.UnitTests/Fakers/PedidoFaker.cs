using Bogus;
using Pedidos.Application.Commands.CriarPedido;
using Pedidos.Application.DTOs.Requests;
using Pedidos.Domain.Entities;

namespace Pedidos.UnitTests.Fakers;

public static class PedidoFaker
{
    private static readonly Faker _faker = new("pt_BR");

    public static Faker<ItemPedidoRequest> ItemPedidoRequestFaker => new Faker<ItemPedidoRequest>("pt_BR")
        .CustomInstantiator(f => new ItemPedidoRequest(
            ProdutoId: f.Random.Int(1, 1000),
            Quantidade: f.Random.Int(1, 10),
            ValorUnitario: f.Finance.Amount(10, 500)
        ));

    public static Faker<CriarPedidoCommand> CriarPedidoCommandFaker => new Faker<CriarPedidoCommand>("pt_BR")
        .CustomInstantiator(f => new CriarPedidoCommand(
            PedidoExternoId: f.Random.Int(1, 99999),
            ClienteId: f.Random.Int(1, 1000),
            Itens: ItemPedidoRequestFaker.Generate(f.Random.Int(1, 5))
        ));

    public static CriarPedidoCommand GerarCommandValido()
    {
        return CriarPedidoCommandFaker.Generate();
    }

    public static CriarPedidoCommand GerarCommandComPedidoExternoId(int pedidoExternoId)
    {
        return CriarPedidoCommandFaker
            .RuleFor(c => c.PedidoExternoId, pedidoExternoId)
            .Generate();
    }

    public static List<ItemPedidoRequest> GerarItensValidos(int quantidade = 2)
    {
        return ItemPedidoRequestFaker.Generate(quantidade);
    }

    public static Pedido GerarPedidoValido()
    {
        var itens = Enumerable.Range(1, _faker.Random.Int(1, 3))
            .Select(_ => ItemPedido.Criar(
                _faker.Random.Int(1, 1000),
                _faker.Random.Int(1, 10),
                _faker.Finance.Amount(10, 500)).Valor!)
            .ToList();

        return Pedido.Criar(
            _faker.Random.Int(1, 99999),
            _faker.Random.Int(1, 1000),
            itens).Valor!;
    }

    public static Pedido GerarPedidoComPedidoExternoId(int pedidoExternoId)
    {
        var itens = new List<ItemPedido>
        {
            ItemPedido.Criar(
                _faker.Random.Int(1, 1000),
                _faker.Random.Int(1, 10),
                _faker.Finance.Amount(10, 500)).Valor!
        };

        return Pedido.Criar(pedidoExternoId, _faker.Random.Int(1, 1000), itens).Valor!;
    }
}
