using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class SlipDO : BaseObject
    {
        [Key]
        public int Id { get; set; }
        
        public string Name { get; set; }


    }
}
