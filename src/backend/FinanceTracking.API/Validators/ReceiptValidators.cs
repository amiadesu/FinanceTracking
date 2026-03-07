using FluentValidation;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Validators;

public class CreateReceiptProductDtoValidator : AbstractValidator<CreateReceiptProductDto>
{
    public CreateReceiptProductDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Categories).NotNull();
    }
}

public class CreateReceiptDtoValidator : AbstractValidator<CreateReceiptDto>
{
    public CreateReceiptDtoValidator()
    {
        RuleFor(x => x.SellerId).NotEmpty();
        RuleFor(x => x.PaymentDate).NotEmpty();
        RuleFor(x => x.Products).NotEmpty().WithMessage("At least one product is required on a receipt.");
        RuleForEach(x => x.Products).SetValidator(new CreateReceiptProductDtoValidator());
    }
}

public class UpdateReceiptProductDtoValidator : AbstractValidator<UpdateReceiptProductDto>
{
    public UpdateReceiptProductDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200).When(x => x.Name != null);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).When(x => x.Price.HasValue);
        RuleFor(x => x.Quantity).GreaterThan(0).When(x => x.Quantity.HasValue);
    }
}

public class UpdateReceiptDtoValidator : AbstractValidator<UpdateReceiptDto>
{
    public UpdateReceiptDtoValidator()
    {
        RuleFor(x => x.SellerId).NotEmpty().When(x => x.SellerId != null);
        RuleForEach(x => x.Products).SetValidator(new UpdateReceiptProductDtoValidator()).When(x => x.Products != null);
    }
}