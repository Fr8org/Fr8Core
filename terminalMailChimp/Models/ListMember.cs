using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalMailChimp.Models
{
    public class Subscriber
    {
        public string ListId { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Status { get; set; }
    }
}