namespace Domain.Users.ValueObjects
{
    public class JWTToken
    {
        public string AccessToken { get; }
        public string RefreshToken { get; }

        public JWTToken(string accessToken, string refreshToken)
        {
            if (string.IsNullOrEmpty(accessToken)) throw new ArgumentException("Access token cannot be null");
            if (string.IsNullOrEmpty(refreshToken)) throw new ArgumentException("Refresh token cannot be null");

            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }
}
