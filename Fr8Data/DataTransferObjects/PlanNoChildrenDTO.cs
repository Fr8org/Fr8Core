using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Fr8Data.States;

namespace Fr8Data.DataTransferObjects
{
    public class PlanNoChildrenDTO
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Tag { get; set; }

        public string Description { get; set; }

        public DateTimeOffset LastUpdated { get; set; }

        public int PlanState { get; set; }

        public Guid StartingSubPlanId { get; set; }

        public PlanVisibility Visibility { get; set; }

        public string Category { get; set; }
    }
}