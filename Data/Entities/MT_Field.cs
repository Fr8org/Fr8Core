using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{

    public enum MT_FieldType
    {
        String,
        Int,
        Boolean
    }

    public class MT_Field
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        [Index("FieldColumnOffsetIndex", 2)]
        public string Name { get; set; }

        [Required]
        public MT_FieldType Type { get; set; }

        [Required, Index("FieldColumnOffsetIndex", 3)]
        public int FieldColumnOffset { get; set; }

        [Required, ForeignKey("MT_Object")]
        [Index("FieldColumnOffsetIndex", 1)]
        public int MT_ObjectId { get; set; }

        public MT_Object MT_Object { get; set; }
    }
}
