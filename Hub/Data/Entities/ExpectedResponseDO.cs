using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;

namespace Data.Entities
{
    public class ExpectedResponseDO : BaseObject
    {
        [Key]
        public int Id { get; set; }

        public String AssociatedObjectType { get; set; }
        public int AssociatedObjectID { get; set; }

        [ForeignKey("User")]
        public String UserID { get; set; }
        public Fr8AccountDO User { get; set; }

        [ForeignKey("StatusTemplate")]
        public int Status { get; set; }
        public _ExpectedResponseStatusTemplate StatusTemplate { get; set; }
    }
}
