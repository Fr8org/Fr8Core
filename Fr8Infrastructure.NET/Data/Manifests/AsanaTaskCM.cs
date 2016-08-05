using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class AsanaTaskCM:Manifest
    {
        public string Id { get; set; }

        public string Assignee { get; set; }

        public string AssigneeStatus { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool Completed { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? DueOn { get; set; }

        public DateTime? DueAt { get; set; }

        public string External { get; set; }

        public IEnumerable<string> Followers { get; set; }

        public bool Hearted { get; set; }

        public IEnumerable<string> Hearts { get; set; }

        public DateTime ModifiedAt { get; set; }

        public string Name { get; set; }

        public string Notes { get; set; }

        public int NumHearts { get; set; }

        public IEnumerable<string> Projects { get; set; }

        public string Parent { get; set; }

        public string Workspace { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public AsanaTaskCM():base(MT.AsanaTask)
        {
            
        }
    }
}
