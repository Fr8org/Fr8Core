using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class AuthenticationTokenGrantSampleFactory : ISwaggerSampleFactory<AuthenticationTokenGrantDTO>
    {
        public AuthenticationTokenGrantDTO GetSampleData()
        {
            return new AuthenticationTokenGrantDTO
            {
                IsMain = true,
                AuthTokenId = Guid.Parse("2FF82BC0-105F-419C-A3EE-D05C37B5D8C1"),
                ActivityId = Guid.Parse("157A2147-5F29-46A1-BAAD-BA1032655ECF")
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}