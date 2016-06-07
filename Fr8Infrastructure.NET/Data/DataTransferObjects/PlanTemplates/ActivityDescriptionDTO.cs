using System;

namespace fr8.Infrastructure.Data.DataTransferObjects.PlanTemplates
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
