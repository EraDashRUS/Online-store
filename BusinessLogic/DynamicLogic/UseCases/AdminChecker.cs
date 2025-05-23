using OnlineStore.BusinessLogic.StaticLogic.Contracts;

namespace OnlineStore.BusinessLogic.DynamicLogic.UseCases
{
    public class AdminChecker : IAdminChecker
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminChecker(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> IsAdminAsync(string email)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"http://localhost:5000/api/Users/is-admin/{email}");
            return response.IsSuccessStatusCode && await response.Content.ReadFromJsonAsync<bool>();
        }
    }
}
