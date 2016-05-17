using System;
using System.Collections.Generic;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;

namespace TerminalBase.Models
{
    public class ActivityPayload
    {
        public Guid Id { get; set; }
        public string Label { get; set; }
        public string Name { get; set; }
        public List<ActivityPayload> ChildrenActivities { get; set; }
        public ActivityTemplateDTO ActivityTemplate { get; set; }
        public ICrateStorage CrateStorage { get; set; }
        public Guid? RootPlanNodeId { get; set; }
        public Guid? ParentPlanNodeId { get; set; }
        public int Ordering { get; set; }
    }
}
