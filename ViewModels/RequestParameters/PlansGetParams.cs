using System;
using Fr8.Infrastructure.Data.States;

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
        /// <summary>
        /// Whether to include plan's activities in the response
        /// </summary>
        public bool include_children { get; set; } = false;     
        
        /// <summary>
        /// Level of visibility (Standard, Internal) to filter plans by
        /// </summary>   
        public PlanVisibility visibility { get; set; } = PlanVisibility.Standard;
    }
}