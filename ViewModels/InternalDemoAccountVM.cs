using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HubWeb.ViewModels
{
    public class InternalDemoAccountVM
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string Domain { get; set; }

        public bool HasDemoAccount { get; set; }
    }
}