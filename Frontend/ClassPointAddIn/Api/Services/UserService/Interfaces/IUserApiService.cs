using System.Threading.Tasks;

namespace ClassPointAddIn.Api.Service
{
    public interface IUserApiService
    {
        Task<LoginResponse> LoginAsync(string username, string password);
        Task<RegisterResponse> RegisterAsync(string username, string email, string password);
    }
}
