using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class AdminEmailFilter : IAsyncAuthorizationFilter
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AdminEmailFilter(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var email = context.HttpContext.User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            context.Result = new ForbidResult();
            return;
        }

        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync($"http://localhost:5000/api/users/is-admin/{email}");
        if (!response.IsSuccessStatusCode || !await response.Content.ReadFromJsonAsync<bool>())
        {
            context.Result = new ForbidResult();
        }
    }
}