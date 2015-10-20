using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class MT_Object
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int ManifestId { get; set; }

        [Required]
        public string Name { get; set; }

        public IEnumerable<MT_Field> Fields { get; set; }

        public MT_FieldType MT_FieldType { get; set; }

    }
}
