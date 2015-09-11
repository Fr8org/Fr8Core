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

    public class MT_FieldDO
    {
        [Key]
        public string Id { get; set; }

        [Required, MaxLength(150)]
        [Index("IX_Object_FieldName_Offset", 2, IsUnique = true)]
        public string Name { get; set; }

        [Required]
        public MT_FieldType Type { get; set; }

        [Required, Index("IX_Object_FieldName_Offset", 3, IsUnique = true)]
        public int FieldColumnOffset { get; set; }

        [Required, ForeignKey("MtObject"), MaxLength(100)]
        [Index("IX_Object_FieldName_Offset", 1)]
        public string MtObjectId { get; set; }

        public MT_ObjectDO MtObject { get; set; }
    }
}
