using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Data.Interfaces.DataTransferObjects
{
    /// <summary>
    /// Data transfer object for SubPlanDO entity.
    /// </summary>
    public class SubPlanDTO
    {
        public Guid? SubPlanId { get; set; }

        public Guid? PlanId { get; set; }

        public Guid? ParentId { get; set; }

        public string Name { get; set; }

        public string TransitionKey { get; set; }

        public bool Runnable { get; set; }
    }
}