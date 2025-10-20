using ClassPointAddIn.Api.Service;
using Domain.Users.Entities;
using Domain.Users.ValueObjects;
using System.Threading.Tasks;

namespace ClassPointAddIn.Users.Auth
{
    public class AuthenticationService
    {
        private readonly IUserApiClient _userApiClient;
        private string _currentToken;

        public AuthenticationService(IUserApiClient userApiClient)
        {
            _userApiClient = userApiClient;
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            var tokenResponse = await _userApiClient.LoginAsync(username, password);

            var token = new JWTToken(tokenResponse.Access, tokenResponse.Refresh);
            _currentToken = tokenResponse.Access; // Store the token

            var user = new User(username);
            user.AssignToken(token);

            return user;
        }

        public string GetCurrentToken()
        {
            return _currentToken;
        }

        public void SetCurrentToken(string token)
        {
            _currentToken = token;
        }

        public void Logout()
        {
            _currentToken = null;
        }
    }
}
