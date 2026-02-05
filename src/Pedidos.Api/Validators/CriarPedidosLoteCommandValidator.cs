using FluentValidation;
using Pedidos.Application.Commands.CriarPedidosLote;

namespace Pedidos.Api.Validators;

public sealed class CriarPedidosLoteCommandValidator : AbstractValidator<CriarPedidosLoteCommand>
{
    public CriarPedidosLoteCommandValidator()
    {
        RuleFor(x => x.Pedidos)
            .NotNull()
            .WithMessage("Lista de pedidos não pode ser nula")
            .NotEmpty()
            .WithMessage("Lista de pedidos não pode estar vazia")
            .Must(p => p.Count <= 1000)
            .WithMessage("Máximo de 1000 pedidos por lote");

        RuleForEach(x => x.Pedidos).ChildRules(pedido =>
        {
            pedido.RuleFor(p => p.PedidoId)
                .GreaterThan(0)
                .WithMessage("PedidoId deve ser maior que zero");

            pedido.RuleFor(p => p.ClienteId)
                .GreaterThan(0)
                .WithMessage("ClienteId deve ser maior que zero");

            pedido.RuleFor(p => p.Itens)
                .NotEmpty()
                .WithMessage("Pedido deve conter pelo menos um item");
        });
    }
}
