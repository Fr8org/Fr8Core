using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class MT_Object
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, ForeignKey("Id")]
        public MT_FieldType MT_FieldType { get; set; }

        [Required, ForeignKey("MT_Organization")]
        public int MT_OrganizationId { get; set; }

        public MT_Organization MT_Organization { get; set; }
    }
}
