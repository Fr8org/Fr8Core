using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects
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
    }
}
