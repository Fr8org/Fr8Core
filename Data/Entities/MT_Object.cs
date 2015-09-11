using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class MT_Object
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), MaxLength(100)]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, ForeignKey("MT_Organization")]
        public string MT_OrganizationId { get; set; }

        public MT_Organization MT_Organization { get; set; }
    }
}
