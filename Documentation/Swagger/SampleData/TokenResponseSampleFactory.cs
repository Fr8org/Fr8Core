using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace HubWeb.Documentation.Swagger
{
    public class TokenResponseSampleFactory : ISwaggerSampleFactory<TokenResponseDTO>
    {
        public TokenResponseDTO GetSampleData()
        {
            return new TokenResponseDTO
            {
                Error = string.Empty,
                AuthTokenId = "5184F18C-4306-46CE-8F43-6DD30D242F74",
                TerminalName = "terminalFr8Core",
                TerminalId = 1
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}