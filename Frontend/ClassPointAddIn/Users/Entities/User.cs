using Domain.Users.ValueObjects;

namespace Domain.Users.Entities
{
    public class User
    {
        public string Username { get; private set; }
        public string Email { get; private set; }
        public JWTToken Token { get; private set; }

        public User(string username)
        {
            Username = username;
        }

        public void AssignToken(JWTToken token)
        {
            Token = token;
        }
    }
}
