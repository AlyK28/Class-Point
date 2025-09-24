using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ClassPointAddIn.Api.Service
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

            // Serialize using Newtonsoft.Json
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("login/", content);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Login failed: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();

            // Deserialize using Newtonsoft.Json
            var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(json);

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

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("register/", content);

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Register failed: {response.StatusCode}");

            var json = await response.Content.ReadAsStringAsync();

            var registerResponse = JsonConvert.DeserializeObject<RegisterResponse>(json);

            return registerResponse;
        }
    }
}
