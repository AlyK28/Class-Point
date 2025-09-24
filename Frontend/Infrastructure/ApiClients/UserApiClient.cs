using Application.Users.Api;
using Application.Users.Api.Response;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Infrastructure.ApiClients
{
    public class UserApiClient : IUserApiClient
    {
        private readonly HttpClient _httpClient;

        public UserApiClient()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("http://localhost:8000/api/users/");
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            var payload = new
            {
                username,
                password
            };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("login/", content);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Login failed: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return loginResponse;
        }

        public async Task<RegisterResponse> RegisterAsync(string username, string email, string password)
        {
            var payload = new
            {
                username,
                email,
                password
            };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("register/", content);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Register failed: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();
            var registerResponse = JsonSerializer.Deserialize<RegisterResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return registerResponse;
        }
    }
}
