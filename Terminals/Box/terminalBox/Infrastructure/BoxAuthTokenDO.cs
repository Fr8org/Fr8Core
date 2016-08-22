using System;

namespace terminalBox.Infrastructure
{
    public class BoxAuthTokenDO
    {
        public string AccessToken { get; }
        public string RefreshToken { get; }
        public DateTime ExpiresAt { get; }

        public BoxAuthTokenDO(string accessToken, string refreshToken, DateTime expiresAt)
        {
            if (accessToken == null)
                throw new ArgumentNullException(nameof(accessToken));
            if (refreshToken == null)
                throw new ArgumentNullException(nameof(refreshToken));

            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ExpiresAt = expiresAt;
        }
    }
}