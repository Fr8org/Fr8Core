using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;

namespace TerminalBase.BaseClasses
{
    public class Activity
    {
        public string Label { get; set; }
        public ActivityTemplate ActivityTemplate { get; set; }
        public Guid? RootPlanNodeId { get; set; }
        public Guid? ParentPlanNodeId { get; set; }
        public string CurrentView { get; set; }
        public int Ordering { get; set; }
        public Guid Id { get; set; }
        public CrateStorage CrateStorage { get; set; }
        public List<Activity> ChildrenActivities { get; set; }
        public AuthorizationToken AuthToken { get; set; }
        public string Fr8AccountId { get; set; }
        public string Documentation { get; set; }
    }



    public class ActivityTemplate
    {
    }

    public class Payload
    {

    }

    public class AuthorizationToken
    {

    }
}
