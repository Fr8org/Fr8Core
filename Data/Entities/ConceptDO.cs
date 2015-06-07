using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Data.Entities
{
    public class ConceptDO : BaseDO
    {
        [Key]
        public int Id { get; set; }
        
        public string Name { get; set; }

        [ForeignKey("Email")]
        public int? RequestId { get; set; }
        public virtual EmailDO Email { get; set; }
    }
}
