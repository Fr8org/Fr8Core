using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace HubWeb.Documentation.Swagger
{
    public class AuthenticationTokenTerminalSampleFactory : ISwaggerSampleFactory<AuthenticationTokenTerminalDTO>
    {
        private readonly ISwaggerSampleFactory<AuthenticationTokenDTO> _authorizationTokenSampleFactory;

        public AuthenticationTokenTerminalSampleFactory(ISwaggerSampleFactory<AuthenticationTokenDTO> authorizationTokenSampleFactory)
        {
            _authorizationTokenSampleFactory = authorizationTokenSampleFactory;
        }

        public AuthenticationTokenTerminalDTO GetSampleData()
        {
            return new AuthenticationTokenTerminalDTO
            {
                Name = "terminalFr8Core",
                Id = 1,
                Label = "Display Name",
                Version = "1",
                AuthenticationType = 0,
                AuthTokens = new List<AuthenticationTokenDTO> { _authorizationTokenSampleFactory.GetSampleData() }
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}