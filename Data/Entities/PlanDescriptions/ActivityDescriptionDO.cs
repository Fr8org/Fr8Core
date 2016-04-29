using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class ActivityDescriptionDO : BaseObject
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        [ForeignKey("ActivityTemplate")]
        public Guid ActivityTemplateId { get; set; }

        public ActivityTemplateDO ActivityTemplate { get; set; }

        public ActivityDescriptionStatus Status { get; set; }

        public string CrateStorage { get; set; }
    }

    public enum ActivityDescriptionStatus { Primary, Inactive, Unavailable, Active }
}
