using FluentValidation;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Validators;

public class CreateSellerDtoValidator : AbstractValidator<CreateSellerDto>
{
    public CreateSellerDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).MaximumLength(100).When(x => x.Name != null);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description != null);
    }
}

public class UpdateSellerDtoValidator : AbstractValidator<UpdateSellerDto>
{
    public UpdateSellerDtoValidator()
    {
        RuleFor(x => x.Name).MaximumLength(100).When(x => x.Name != null);
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description != null);
    }
}