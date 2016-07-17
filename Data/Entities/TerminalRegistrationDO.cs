using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class TerminalRegistrationDO : BaseObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Endpoint { get; set; }

        [ForeignKey("User")]
        public String UserId { get; set; }
        public Fr8AccountDO User { get; set; }
        public bool IsFr8OwnTerminal { get; set; }
    }
}
