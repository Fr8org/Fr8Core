using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Fr8Data.States;

namespace HubWeb.ViewModels.RequestParameters
{
    public class PlansGetParams
    {
        public string name { get; set; }
        public Guid? id { get; set; }
        public Guid? activity_id { get; set; }
        public bool include_children { get; set; } = false;
        public PlanVisibility visibility { get; set; } = PlanVisibility.Standard;
    }
}