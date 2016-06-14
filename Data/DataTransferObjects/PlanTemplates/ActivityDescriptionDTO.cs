using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects.PlanTemplates
{
    public class ActivityDescriptionDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public string OriginalId { get; set; }

        public string CrateStorage { get; set; }

        public Guid ActivityTemplateId { get; set; }
    }
}
