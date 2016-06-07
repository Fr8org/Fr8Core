using System;
using fr8.Infrastructure.Data.States;

namespace HubWeb.ViewModels.RequestParameters
{
    /// <summary>
    /// Parameters for GET method of PlansController
    /// </summary>
    public class PlansGetParams
    {
        public string name { get; set; }
        public Guid? id { get; set; }
        public Guid? activity_id { get; set; }
        public bool include_children { get; set; } = false;        
        public PlanVisibility visibility { get; set; } = PlanVisibility.Standard;
    }
}