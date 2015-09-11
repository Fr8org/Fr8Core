using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class MT_OrganizationDO
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
