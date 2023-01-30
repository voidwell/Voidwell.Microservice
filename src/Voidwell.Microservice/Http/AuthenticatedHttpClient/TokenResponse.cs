using System;

namespace Voidwell.Microservice.Http.AuthenticatedHttpClient
{
    public class TokenState
    {
        public TokenState(string accessToken, int expiresIn)
        {
            AccessToken = accessToken;
            Expiration = DateTimeOffset.UtcNow.AddSeconds(expiresIn);
        }

        public string AccessToken { get; private set; }
        public DateTimeOffset Expiration { get; private set; }

        public bool IsExpired()
        {
            return string.IsNullOrWhiteSpace(AccessToken) || DateTime.UtcNow > Expiration.AddMinutes(-5);
        }
    }
}
