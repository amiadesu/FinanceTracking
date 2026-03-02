using FluentValidation;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Validators;

public class CreateBudgetGoalDtoValidator : AbstractValidator<CreateBudgetGoalDto>
{
    public CreateBudgetGoalDtoValidator()
    {
        RuleFor(x => x.TargetAmount).GreaterThan(0).WithMessage("Target amount must be greater than zero.");
        RuleFor(x => x.StartDate).NotEmpty().WithMessage("Start date is required.");
        RuleFor(x => x.EndDate).NotEmpty().WithMessage("End date is required.")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after the start date.");
    }
}

public class UpdateBudgetGoalDtoValidator : AbstractValidator<UpdateBudgetGoalDto>
{
    public UpdateBudgetGoalDtoValidator()
    {
        RuleFor(x => x.TargetAmount).GreaterThan(0).When(x => x.TargetAmount.HasValue)
            .WithMessage("Target amount must be greater than zero.");
        
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be after the start date.");
    }
}