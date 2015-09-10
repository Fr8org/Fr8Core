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

        [Required]
        public string Name { get; set; }

        [Required]
        public MT_FieldType Type { get; set; }

        [Required, Index("IX_OrganizationObjectOffset", 2, IsUnique = true)]
        public int FieldColumnOffset { get; set; }

        [Required, ForeignKey("MtObject")]
        [Index("IX_OrganizationObjectOffset", 1)]
        public string MtObjectId { get; set; }

        public MT_ObjectDO MtObject { get; set; }
    }
}
