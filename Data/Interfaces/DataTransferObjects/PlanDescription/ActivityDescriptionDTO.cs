using Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Interfaces.DataTransferObjects.PlanDescription
{
    public class ActivityDescriptionDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public Guid ActivityTemplateId { get; set; }
    }
}
