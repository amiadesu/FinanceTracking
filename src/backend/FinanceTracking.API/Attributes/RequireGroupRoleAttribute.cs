using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FinanceTracking.API.Extensions;
using FinanceTracking.API.Models;
using FinanceTracking.API.Services;

namespace FinanceTracking.API.Attributes;

public class RequireGroupRoleAttribute : Attribute, IAsyncActionFilter
{
    private readonly GroupRole _requiredRole;

    public RequireGroupRoleAttribute(GroupRole requiredRole)
    {
        _requiredRole = requiredRole;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var groupIdParam = context.ActionArguments.FirstOrDefault(x => x.Key == "groupId").Value;
        
        if (groupIdParam == null || !int.TryParse(groupIdParam.ToString(), out var groupId))
        {
            context.Result = new BadRequestObjectResult(new { message = Constants.ErrorMessages.InvalidGroupId });
            return;
        }

        var groupService = context.HttpContext.RequestServices.GetRequiredService<GroupService>();
        var userId = context.HttpContext.User.GetUserId();
        
        var userRole = await groupService.GetUserRoleInGroupAsync(groupId, userId);
        
        // User is not a member
        if (userRole == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        // Check if user's role is high enough (lower enum value = higher privilege)
        if (userRole.Value > _requiredRole)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
