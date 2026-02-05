using FluentValidation;
using Pedidos.Application.Commands.CriarPedido;

namespace Pedidos.Api.Validators;

public sealed class CriarPedidoCommandValidator : AbstractValidator<CriarPedidoCommand>
{
    public CriarPedidoCommandValidator()
    {
        RuleFor(x => x.PedidoExternoId)
            .GreaterThan(0)
            .WithMessage("PedidoExternoId deve ser maior que zero");

        RuleFor(x => x.ClienteId)
            .GreaterThan(0)
            .WithMessage("ClienteId deve ser maior que zero");

        RuleFor(x => x.Itens)
            .NotNull()
            .WithMessage("Pedido deve conter itens")
            .NotEmpty()
            .WithMessage("Pedido deve conter pelo menos um item");

        RuleForEach(x => x.Itens)
            .SetValidator(new ItemPedidoRequestValidator());
    }
}
