using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Fr8.Infrastructure.Data.States;

namespace Fr8.Infrastructure.Data.DataTransferObjects
{
    public class PlanEmptyDTO
    {
        public Guid Id { get; set; }

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