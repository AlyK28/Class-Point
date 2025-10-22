using ClassPointAddIn.Api.Service;
using ClassPointAddIn.Api.Service.ClassPointAddIn.Api.Service;
using Domain.Users.Entities;
using Domain.Users.ValueObjects;
using System.Threading.Tasks;

namespace ClassPointAddIn.Users.Auth
{
    public class AuthenticationService
    {
        private readonly IUserApiService _userApiClient;

        public AuthenticationService(IUserApiService userApiClient)
        {
            _userApiClient = userApiClient;
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            var tokenResponse = await _userApiClient.LoginAsync(username, password);

            var token = new JWTToken(tokenResponse.Access, tokenResponse.Refresh);

            BaseApiClient.SetGlobalTokens(token.AccessToken, token.RefreshToken);

            var user = new User(username);
            user.AssignToken(token);

            return user;
        }
    }
}
