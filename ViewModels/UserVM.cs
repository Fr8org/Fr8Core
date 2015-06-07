using System;
using System.Collections.Generic;

namespace KwasantWeb.ViewModels
{
    public class UserVM
    {
        public UserVM()
        {
            Calendars = new List<UserCalendarVM>();
        }

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
        public List<UserCalendarVM> Calendars { get; set; }
    }

    public class UserCalendarVM
    {
        public int Id { get; set; }
        public String Name { get; set; }
    }
}