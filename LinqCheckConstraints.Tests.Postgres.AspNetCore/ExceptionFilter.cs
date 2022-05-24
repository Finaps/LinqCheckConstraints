using Finaps.LinqCheckConstraints.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LinqCheckConstraints.Tests.AspNetCore;

public class ExceptionFilter : IActionFilter
{
  public void OnActionExecuting(ActionExecutingContext context) { }

  public void OnActionExecuted(ActionExecutedContext context)
  {
    if (context.Result is ObjectResult {Value: null})
      context.Result = new NotFoundResult();

    switch (context.Exception)
    {
      case ConstraintException check:
        context.Result = new BadRequestObjectResult(new ValidationProblemDetails(
          check.Properties.ToDictionary(property => property, _ => new []{ check.Message })));
        context.Exception = null;
        break;
    }
  }
}