using FluentValidation;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Validators;

public class CreateInvitationDtoValidator : AbstractValidator<CreateInvitationDto>
{
    public CreateInvitationDtoValidator()
    {
        RuleFor(x => x.TargetUserIdentifier).NotEmpty().WithMessage("Target user identifier is required.");
        RuleFor(x => x.Note).MaximumLength(500).When(x => x.Note != null);
    }
}