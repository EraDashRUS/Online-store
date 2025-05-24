using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using OnlineStore.BusinessLogic.StaticLogic.Contracts;

/// <summary>
/// Фильтр авторизации, ограничивающий доступ только администраторам.
/// </summary>
public class AdminEmailFilter : IAsyncAuthorizationFilter
{
    private readonly IAdminChecker _adminChecker;

    /// <summary>
    /// Конструктор фильтра, принимающий сервис проверки администратора.
    /// </summary>
    /// <param name="adminChecker">Сервис проверки прав администратора.</param>
    public AdminEmailFilter(IAdminChecker adminChecker)
    {
        _adminChecker = adminChecker;
    }

    /// <summary>
    /// Выполняет проверку, является ли пользователь администратором.
    /// В случае отсутствия прав возвращает результат Forbid.
    /// </summary>
    /// <param name="context">Контекст фильтра авторизации.</param>
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!await _adminChecker.IsAdminAsync())
        {
            context.Result = new ForbidResult();
        }
    }
}