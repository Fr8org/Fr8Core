using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class AuthenticationTokenSampleFactory : ISwaggerSampleFactory<AuthenticationTokenDTO>
    {
        public AuthenticationTokenDTO GetSampleData()
        {
            return new AuthenticationTokenDTO
            {
                Id = Guid.Parse("9B5DE767-2F94-479F-9B46-4FDD1248954B"),
                ExternalAccountName = "Fr8 User",
                IsSelected = false,
                IsMain = true
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}