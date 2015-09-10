using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class MT_DataDO
    {
        [Required]
        public string GUID { get; set; }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        [Required, ForeignKey("MtObject")]
        public string MtObjectId { get; set; }

        public MT_ObjectDO MtObject { get; set; }

        public string Value1 { get; set; }

        public string Value2 { get; set; }

        public string Value3 { get; set; }

        public string Value4 { get; set; }

        public string Value5 { get; set; }

        public string Value6 { get; set; }

        public string Value7 { get; set; }

        public string Value8 { get; set; }

        public string Value9 { get; set; }

        public string Value10 { get; set; }
    }
}
