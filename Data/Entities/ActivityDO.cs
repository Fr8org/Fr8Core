using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public abstract class ActivityDO : BaseDO
    {
        [Key]
        public int Id { get; set; }

        public int Ordering { get; set; }
    }
}