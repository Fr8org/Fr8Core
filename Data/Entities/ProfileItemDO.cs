using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class ProfileItemDO : BaseDO
    {
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("ProfileNode"), Required]
        public int? ProfileNodeID { get; set; }
        public virtual ProfileNodeDO ProfileNode { get; set; }

        public String Key { get; set; }
        public String Value { get; set; }
    }
}
