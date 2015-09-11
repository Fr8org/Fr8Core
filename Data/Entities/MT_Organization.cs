using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class MT_Organization
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), MaxLength(100)]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
