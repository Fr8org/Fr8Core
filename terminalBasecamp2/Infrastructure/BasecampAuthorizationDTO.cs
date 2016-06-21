using System;

namespace terminalBasecamp.Infrastructure
{
    public class BasecampAuthorizationDTO
    {
        public DateTime ExpiresAt { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }
    }
}