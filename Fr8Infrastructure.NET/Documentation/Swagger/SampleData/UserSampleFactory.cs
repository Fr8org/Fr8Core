using System;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Documentation.Swagger
{
    public class UserSampleFactory : ISwaggerSampleFactory<UserDTO>
    {
        public UserDTO GetSampleData()
        {
            return new UserDTO
            {
                Id = "E9672535-ACDE-4799-9756-5B233DD67B8B",
                Status = 1,
                EmailAddress = "youraddress@yourdomain.com",
                EmailAddressID = 1,
                FirstName = "John",
                LastName = "Doe",
                ProfileId = Guid.Parse("5A22A7FF-FAC8-4B45-A086-EE21E79D699E"),
                Role = "StandardUser"
            };
        }

        object ISwaggerSampleFactory.GetSampleData()
        {
            return GetSampleData();
        }
    }
}