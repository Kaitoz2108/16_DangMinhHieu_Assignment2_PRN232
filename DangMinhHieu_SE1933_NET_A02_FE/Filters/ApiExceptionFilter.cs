using FUNewsManagement_Client.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FUNewsManagement_Client.Filters;

public class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is UnauthorizedApiException)
        {
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
            context.ExceptionHandled = true;
        }
        else if (context.Exception is ForbiddenApiException)
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            context.ExceptionHandled = true;
        }
    }
}
