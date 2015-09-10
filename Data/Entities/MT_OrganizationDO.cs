using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class MT_OrganizationDO
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
