namespace ClassPointAddIn.Api.Service
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;

    namespace ClassPointAddIn.Api.Service
    {
        public abstract class BaseApiClient
        {
            private static string _accessToken;
            private static string _refreshToken;
            protected readonly HttpClient _httpClient;
            private static readonly HttpClient _authClient;

            // Configure JSON settings for snake_case serialization
            private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                NullValueHandling = NullValueHandling.Ignore
            };

            static BaseApiClient()
            {
                _authClient = new HttpClient { BaseAddress = new Uri("http://localhost:8000/api/auth/") };
                _authClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            protected BaseApiClient(string baseUrl)
            {
                _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8000/" + baseUrl) };
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            public static void SetGlobalTokens(string access, string refresh)
            {
                _accessToken = access;
                _refreshToken = refresh;
            }

            private void ApplyAuthorizationHeader()
            {
                if (!string.IsNullOrEmpty(_accessToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _accessToken);
                }
            }

            protected async Task<TResponse> GetAsync<TResponse>(string endpoint)
                => await SendAsync<TResponse>(() => _httpClient.GetAsync(endpoint));

            protected async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest payload)
            {
                var content = new StringContent(JsonConvert.SerializeObject(payload, JsonSettings), Encoding.UTF8, "application/json");
                return await SendAsync<TResponse>(() => _httpClient.PostAsync(endpoint, content));
            }

            protected async Task<TResponse> PatchAsync<TRequest, TResponse>(string endpoint, TRequest payload)
            {
                var content = new StringContent(JsonConvert.SerializeObject(payload, JsonSettings), Encoding.UTF8, "application/json");
                return await SendAsync<TResponse>(() =>
                {
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint)
                    {
                        Content = content
                    };
                    return _httpClient.SendAsync(request);
                });
            }

            private async Task<TResponse> SendAsync<TResponse>(Func<Task<HttpResponseMessage>> send)
            {
                ApplyAuthorizationHeader();
                var response = await send();

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && await TryRefreshTokenAsync())
                {
                    ApplyAuthorizationHeader();
                    response = await send(); // retry once
                }

                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Request failed: {response.StatusCode} - {json}");

                return JsonConvert.DeserializeObject<TResponse>(json, JsonSettings);
            }

            private async Task<bool> TryRefreshTokenAsync()
            {
                if (string.IsNullOrEmpty(_refreshToken))
                    return false;

                var payload = new { refresh = _refreshToken };
                var content = new StringContent(JsonConvert.SerializeObject(payload, JsonSettings), Encoding.UTF8, "application/json");
                var response = await _authClient.PostAsync("token/refresh/", content);

                if (!response.IsSuccessStatusCode)
                    return false;

                var json = await response.Content.ReadAsStringAsync();
                dynamic data = JsonConvert.DeserializeObject(json, JsonSettings);
                _accessToken = data.access;
                return true;
            }
        }
    }
}


