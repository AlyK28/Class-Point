using ClassPointAddIn.Api.Service.ClassPointAddIn.Api.Service;
using System.Threading.Tasks;

namespace ClassPointAddIn.Api.Service
{
    public class UserApiService : BaseApiClient, IUserApiService
    {
        public UserApiService() : base("api/users/") { }

        public Task<LoginResponse> LoginAsync(string username, string password)
        {
            var payload = new { username, password };
            return PostAsync<object, LoginResponse>("login/", payload);
        }

        public Task<RegisterResponse> RegisterAsync(string username, string email, string password)
        {
            var payload = new { username, email, password };
            return PostAsync<object, RegisterResponse>("register/", payload);
        }
    }
}
