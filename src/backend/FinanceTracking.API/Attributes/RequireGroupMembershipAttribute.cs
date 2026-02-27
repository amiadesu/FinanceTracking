using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FinanceTracking.API.Extensions;
using FinanceTracking.API.Services;

namespace FinanceTracking.API.Attributes;

public class RequireGroupMembershipAttribute : Attribute, IAsyncActionFilter
{
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
        
        var isMember = await groupService.IsUserActiveMemberAsync(groupId, userId);
        
        if (!isMember)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}