using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class AuthorizationTokenSampleFactory : ISwaggerSampleFactory<AuthorizationTokenDTO>
    {
        public AuthorizationTokenDTO GetSampleData()
        {
            return new AuthorizationTokenDTO
            {
                Id = "7231128B-BA7C-4F73-BAFE-A98219A649BD",
                ExternalAccountName = "Your Name",
                AdditionalAttributes = "api_version=22",
                AuthCompletedNotificationRequired = false,
                Error = string.Empty,
                ExpiresAt = DateTime.Today.AddDays(30.0),
                ExternalAccountId = "A123456",
                ExternalDomainId = "D123456",
                ExternalDomainName = "somedomain",
                ExternalStateToken = "CBFF1688-99E8-4FC2-A526-C02D98CB5944",
                TerminalID = Guid.Parse("B125F7D4-7A3F-47B2-90D5-932AC1CE5F48"),
                Token = "QWERTYUIOPASDFGGHJKLZXCBNM",
                UserId = "6115F7D4-7A3F-47B2-90D5-932AC1CE5F48"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}