using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HubWeb.ViewModels.RequestParameters
{
    public class PlansPostParams
    {
        public string solution_name { get; set; }
        public bool update_registrations { get; set; } = false;
    }
}