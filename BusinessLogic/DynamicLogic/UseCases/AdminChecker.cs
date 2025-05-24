using OnlineStore.BusinessLogic.StaticLogic.Contracts;

namespace OnlineStore.BusinessLogic.DynamicLogic.UseCases
{
    /// <summary>
    /// Сервис для проверки, является ли текущий пользователь администратором.
    /// </summary>
    /// <remarks>
    /// Инициализирует новый экземпляр класса <see cref="AdminChecker"/>.
    /// </remarks>
    /// <param name="httpClientFactory">Фабрика HTTP-клиентов.</param>
    /// <param name="httpContextAccessor">Аксессор к текущему HTTP-контексту.</param>
    public class AdminChecker(
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor) : IAdminChecker
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        /// <summary>
        /// Проверяет, является ли текущий пользователь администратором.
        /// </summary>
        /// <returns>True, если пользователь — администратор, иначе false.</returns>
        public async Task<bool> IsAdminAsync()
        {
            var email = GetCurrentUserEmail();
            if (string.IsNullOrEmpty(email))
                return false;

            return await CheckAdminByEmailAsync(email);
        }

        private string? GetCurrentUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User.Identity?.Name;
        }

        private async Task<bool> CheckAdminByEmailAsync(string email)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"http://localhost:5000/api/Users/is-admin/{email}");
            if (!response.IsSuccessStatusCode)
                return false;

            var isAdmin = await response.Content.ReadFromJsonAsync<bool>();
            return isAdmin;
        }
    }
}
