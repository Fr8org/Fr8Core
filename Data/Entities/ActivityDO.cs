using Data.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class ActivityDO : BaseDO
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ParentActivity")]
        public int? ParentActivityId { get; set; }
        
        public virtual ActivityDO ParentActivity { get; set; }

        [InverseProperty("ParentActivity")]
        public virtual List<ActivityDO> Activities { get; set; }

        public int Ordering { get; set; }
    }
}