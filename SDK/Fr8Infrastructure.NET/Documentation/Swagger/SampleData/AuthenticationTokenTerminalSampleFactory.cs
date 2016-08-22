using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
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
                Id = Guid.Parse("2757F870-A508-429E-A706-9EE826D92237"),
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