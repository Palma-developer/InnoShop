using FluentValidation;
using ProductService.Service.Abstraction.Models;


namespace ProductService.Application.Abstraction.Validators
{
    public class CreateProductValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Название продукта обязательно.")
                .MaximumLength(200).WithMessage("Название не может превышать 200 символов.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Цена должна быть больше 0.");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Описание не может превышать 2000 символов.");
        }
    }
}