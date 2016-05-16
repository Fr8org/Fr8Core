using System;
using Newtonsoft.Json;

namespace Fr8Data.DataTransferObjects
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }

        public int EmailAddressID { get; set; }
        public string EmailAddress { get; set; }

        public int Status { get; set; }

        public string Role { get; set; }

        public int? organizationId { get; set; }

        public Guid ProfileId { get; set; }
        public string Class { get; set; }
    }
}
