using System;
using fr8.Infrastructure.Data.States;

namespace HubWeb.ViewModels.RequestParameters
{
    /// <summary>
    /// Parameters for GET method of PlansController
    /// </summary>
    public class PlansGetParams
    {
        /// <summary>
        /// Name of Plan
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Id of Plan
        /// </summary>
        public Guid? id { get; set; }

        /// <summary>
        /// Id of Activity in Plan
        /// </summary>
        public Guid? activity_id { get; set; }

        public bool include_children { get; set; } = false;        
        public PlanVisibility visibility { get; set; } = PlanVisibility.Standard;
    }
}