using FluentValidation;
using OnlineStore.DTOs;

public class CartItemCreateDtoValidator : AbstractValidator<CartItemCreateDto>
{
    public CartItemCreateDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
