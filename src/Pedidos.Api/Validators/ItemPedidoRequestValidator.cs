using FluentValidation;
using Pedidos.Application.DTOs.Requests;

namespace Pedidos.Api.Validators;

public sealed class ItemPedidoRequestValidator : AbstractValidator<ItemPedidoRequest>
{
    public ItemPedidoRequestValidator()
    {
        RuleFor(x => x.ProdutoId)
            .GreaterThan(0)
            .WithMessage("ProdutoId deve ser maior que zero");

        RuleFor(x => x.Quantidade)
            .GreaterThan(0)
            .WithMessage("Quantidade deve ser maior que zero");

        RuleFor(x => x.ValorUnitario)
            .GreaterThan(0)
            .WithMessage("Valor unitário deve ser maior que zero");
    }
}
