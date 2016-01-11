using Data.States.Templates;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities
{
    public class TerminalSubscriptionDO : BaseObject
    {
        public TerminalSubscriptionDO()
        {
            this.SubscriptionState = States.SubscriptionState.Inactive;
        }

        public virtual Fr8AccountDO UserDO { get; set; }
        public int Id { get; set; }

        [ForeignKey("Terminal")]
        public Guid TerminalId { get; set; }
        public virtual TerminalDO Terminal { get; set; }

        [Required]
        [ForeignKey("SubscriptionStateTemplate")]
        public int SubscriptionState { get; set; }

        public _SubscriptionStateTemplate SubscriptionStateTemplate { get; set; }
    }
}
