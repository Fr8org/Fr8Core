using System;
using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class ActivityCategoryDO : BaseObject
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }
        public string IconPath { get; set; }
    }
}
