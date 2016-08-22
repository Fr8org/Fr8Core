using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class ActivityCategorySetDO : BaseObject
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("ActivityCategory")]
        public Guid ActivityCategoryId { get; set; }
        public virtual ActivityCategoryDO ActivityCategory { get; set; }

        [ForeignKey("ActivityTemplate")]
        public Guid ActivityTemplateId { get; set; }
        public virtual ActivityTemplateDO ActivityTemplate { get; set; }
    }
}
