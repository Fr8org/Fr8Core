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
        public Guid Id { get; set; }

        public Guid? PlanId { get; set; }

        public string Name { get; set; }

        public string TransitionKey { get; set; }
    }
}