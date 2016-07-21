using System;
using System.Collections.Generic;
using Fr8.Infrastructure.Data.Constants;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class StandardFr8PlansCM : Manifest
    {
        public DateTime CreateDate { get; set; }

        public DateTime LastUpdated { get; set; }

        public String Description { get; set; }

        public String Name { get; set; }

        public int Ordering { get; set; }

        public String PlanState { get; set; }

        public List<SubplanDTO> SubPlans { get; set; }

        public StandardFr8PlansCM()
            : base(MT.StandardFr8Plans)
        {
        }
    }
}
