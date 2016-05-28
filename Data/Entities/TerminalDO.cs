using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using Data.States.Templates;

namespace Data.Entities
{
    public class TerminalDO : BaseObject, ITerminalDO
    {
        public TerminalDO()
        {
            this.AuthenticationType = Fr8Data.States.AuthenticationType.None;      
        }

        [Key]
        public int Id { get; set; }

        public string PublicIdentifier { get; set; }

        public string Secret { get; set; }
        
        [Required]
        public string Name { get; set; }

        [Required]
        public string Version { get; set; }

        [Required]
        public string Label { get; set; }

        [ForeignKey("TerminalStatusTemplate")]
        public int TerminalStatus { get; set; }
        public _TerminalStatusTemplate TerminalStatusTemplate { get; set; }

        // TODO: remove this, DO-1397
        // public bool RequiresAuthentication { get; set; }

        //public string BaseEndPoint { get; set; }

        public string Endpoint { get; set; }
        
        public virtual Fr8AccountDO UserDO { get; set; }

        public string Description { get; set; }

        [Required]
        [ForeignKey("AuthenticationTypeTemplate")]
        public int AuthenticationType { get; set; }

        public virtual _AuthenticationTypeTemplate AuthenticationTypeTemplate { get; set; }
    }
}
