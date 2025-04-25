using FluentValidation;
using OnlineStore.BusinessLogic.StaticLogic.DTOs; 

namespace OnlineStore.BusinessLogic.StaticLogic.Validators
{
    public class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
    {
        public ProductCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Название обязательно")
                .MaximumLength(100).WithMessage("Максимум 100 символов");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Цена должна быть больше 0");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Максимум 500 символов");
        }
    }
}