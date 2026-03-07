using FluentValidation;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Validators;

public class CreateGroupDtoValidator : AbstractValidator<CreateGroupDto>
{
    public CreateGroupDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateGroupDtoValidator : AbstractValidator<UpdateGroupDto>
{
    public UpdateGroupDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class ResetGroupDtoValidator : AbstractValidator<ResetGroupDto>
{
    public ResetGroupDtoValidator()
    {
        RuleFor(x => x)
            .Must(x => x.ResetMembers || x.ResetBudgetGoals || x.ResetCategories || x.ResetReceiptsProductsAndSellers)
            .WithMessage("At least one reset option must be true.");
    }
}