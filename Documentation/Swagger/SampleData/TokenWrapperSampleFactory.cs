using Fr8.Infrastructure.Documentation.Swagger;
using HubWeb.Controllers;

namespace HubWeb.Documentation.Swagger
{
    public class TokenWrapperSampleFactory : ISwaggerSampleFactory<TokenWrapper>
    {
        public TokenWrapper GetSampleData()
        {
            return new TokenWrapper {Token = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890"};
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}