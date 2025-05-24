using OnlineStore.BusinessLogic.StaticLogic.Contracts;

namespace OnlineStore.BusinessLogic.DynamicLogic.UseCases
{
    public class AdminChecker : IAdminChecker
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminChecker(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> IsAdminAsync()
        {
            var email = _httpContextAccessor.HttpContext?.User.Identity?.Name;
            if (string.IsNullOrEmpty(email))
                return false;

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"http://localhost:5000/api/Users/is-admin/{email}");
            return response.IsSuccessStatusCode && await response.Content.ReadFromJsonAsync<bool>();
        }
    }
}
