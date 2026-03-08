using FluentValidation;
using FinanceTracking.API.DTOs;
using FinanceTracking.API.Constants;
using FinanceTracking.API.Validators;

namespace FinanceTracking.API.Validators;

public class StatisticsFilterDtoValidator : AbstractValidator<StatisticsFilterDto>
{
    public StatisticsFilterDtoValidator()
    {
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("StartDate cannot be later than EndDate.");

        RuleFor(x => x.SellerId)
            .Must(id => InputValidator.IsNumericString(id!))
            .When(x => !string.IsNullOrWhiteSpace(x.SellerId))
            .WithMessage(ErrorMessages.InvalidSellerId);

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .When(x => x.CategoryId.HasValue)
            .WithMessage("CategoryId must be greater than 0.");

        RuleFor(x => x.Top)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Top parameter must be between 1 and 100.");
    }
}