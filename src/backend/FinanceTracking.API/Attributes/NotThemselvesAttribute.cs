using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FinanceTracking.API.Extensions;
using FinanceTracking.API.Models;
using FinanceTracking.API.Services;

namespace FinanceTracking.API.Attributes;

public class NotThemselvesAttribute : Attribute, IAsyncActionFilter
{
    public NotThemselvesAttribute()
    {
        
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userIdParam = context.ActionArguments.FirstOrDefault(x => x.Key == "userId").Value;
        
        if (userIdParam == null || !Guid.TryParse(userIdParam.ToString(), out var userId))
        {
            context.Result = new BadRequestObjectResult(new { message = Constants.ErrorMessages.InvalidUserId });
            return;
        }

        var groupMemberService = context.HttpContext.RequestServices.GetRequiredService<GroupMemberService>();
        var actionUserId = context.HttpContext.User.GetUserId();
        
        if (actionUserId == userId)
        {
            context.Result = new BadRequestObjectResult(new { message = Constants.ErrorMessages.CannotPerformActionOnYourself });
            return;
        }

        await next();
    }
}
