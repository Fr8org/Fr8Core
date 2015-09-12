using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class MT_Data
    {
        [Required, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid GUID { get; set; }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        [Required, ForeignKey("MT_Object")]
        public int MT_ObjectId { get; set; }

        public MT_Object MT_Object { get; set; }

        public bool IsDeleted { get; set; }

        #region Value1 - Value50

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

        public string Value11 { get; set; }

        public string Value12 { get; set; }

        public string Value13 { get; set; }

        public string Value14 { get; set; }

        public string Value15 { get; set; }

        public string Value16 { get; set; }

        public string Value17 { get; set; }

        public string Value18 { get; set; }

        public string Value19 { get; set; }

        public string Value20 { get; set; }

        public string Value21 { get; set; }

        public string Value22 { get; set; }

        public string Value23 { get; set; }

        public string Value24 { get; set; }

        public string Value25 { get; set; }

        public string Value26 { get; set; }

        public string Value27 { get; set; }

        public string Value28 { get; set; }

        public string Value29 { get; set; }

        public string Value30 { get; set; }

        public string Value31 { get; set; }

        public string Value32 { get; set; }

        public string Value33 { get; set; }

        public string Value34 { get; set; }

        public string Value35 { get; set; }

        public string Value36 { get; set; }

        public string Value37 { get; set; }

        public string Value38 { get; set; }

        public string Value39 { get; set; }

        public string Value40 { get; set; }

        public string Value41 { get; set; }

        public string Value42 { get; set; }

        public string Value43 { get; set; }

        public string Value44 { get; set; }

        public string Value45 { get; set; }

        public string Value46 { get; set; }

        public string Value47 { get; set; }

        public string Value48 { get; set; }

        public string Value49 { get; set; }

        public string Value50 { get; set; }

        #endregion
    }
}
