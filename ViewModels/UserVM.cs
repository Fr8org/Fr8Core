using System;

namespace HubWeb.ViewModels
{
    public class UserVM
    {
        public String Id { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String UserName { get; set; }
        public String EmailAddress { get; set; }
        public String NewPassword { get; set; }
        public int Status { get; set; }
        public int EmailAddressID { get; set; }
        public bool SendMail { get; set; }

        public String Role { get; set; }
    }
}