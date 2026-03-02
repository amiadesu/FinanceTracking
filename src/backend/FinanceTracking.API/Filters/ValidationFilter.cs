using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FluentValidation;

namespace FinanceTracking.API.Filters;

public class ValidationFilter<T> : IAsyncActionFilter where T : class
{
    private readonly IValidator<T> _validator;

    public ValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Find the DTO in the incoming request arguments
        var argument = context.ActionArguments.Values.FirstOrDefault(v => v is T);

        if (argument == null)
        {
            context.Result = new BadRequestObjectResult("Invalid request payload.");
            return;
        }

        var validationResult = await _validator.ValidateAsync((T)argument);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => new { Field = e.PropertyName, Error = e.ErrorMessage });
            
            context.Result = new BadRequestObjectResult(errors);
            return;
        }

        await next();
    }
}