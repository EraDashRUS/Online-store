using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;

public class AdminEmailFilter : IAsyncAuthorizationFilter
{
    private readonly IAdminChecker _adminChecker;

    public AdminEmailFilter(IAdminChecker adminChecker)
    {
        _adminChecker = adminChecker;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var email = context.HttpContext.User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email) || !await _adminChecker.IsAdminAsync(email))
        {
            context.Result = new ForbidResult();
        }
    }
}