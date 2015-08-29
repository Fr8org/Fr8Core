using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public abstract class ActivityDO : BaseDO
    {
        [Key]
        public int Id { get; set; }

        public int Ordering { get; set; }
    }
}
