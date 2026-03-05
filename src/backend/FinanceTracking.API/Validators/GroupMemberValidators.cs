using FluentValidation;
using FinanceTracking.API.DTOs;

namespace FinanceTracking.API.Validators;

public class UpdateGroupMemberRoleDtoValidator : AbstractValidator<UpdateGroupMemberRoleDto>
{
    public UpdateGroupMemberRoleDtoValidator()
    {
        RuleFor(x => x.Role).IsInEnum().When(x => x.Role.HasValue)
            .WithMessage("Invalid role specified.");
    }
}