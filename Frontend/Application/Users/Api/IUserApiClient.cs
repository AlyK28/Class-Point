using Application.Users.Api.Response;

namespace Application.Users.Api
{
    public interface IUserApiClient
    {
        Task<LoginResponse> LoginAsync(string username, string password);
        Task<RegisterResponse> RegisterAsync(string username, string email, string password);
    }
}
