using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class MT_ObjectDO
    {
        [Key, MaxLength(100)]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, ForeignKey("MtOrganization")]
        public string MtOrganizationId { get; set; }

        public MT_OrganizationDO MtOrganization { get; set; }
    }
}
