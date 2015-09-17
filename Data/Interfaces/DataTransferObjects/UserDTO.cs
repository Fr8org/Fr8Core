using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects
{
    public class UserDTO
    {
        public String Id { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String UserName { get; set; }

        public int EmailAddressID { get; set; }
        public String EmailAddress { get; set; }

        public int Status { get; set; }

        public String Role { get; set; }
    }
}
