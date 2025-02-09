using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace PlusAppointment.Utils.Errors;

public class ValidationErrorFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(kv => kv.Value.Errors.Any())
                .SelectMany(kv => kv.Value.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            context.Result = new BadRequestObjectResult(new
            {
                error = "Validation Error",
                message = errors
            });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}