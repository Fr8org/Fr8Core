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
    public class SubscriptionDO : BaseObject, ISubscriptionDO
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("DockyardAccount")]
        public string DockyardAccountId { get; set; }
        public virtual Fr8AccountDO DockyardAccount { get; set; }

        [ForeignKey("Terminal")]
        public Guid TerminalId { get; set; }
        public virtual TerminalDO Terminal { get; set; }

        [ForeignKey("AccessLevelTemplate")]
        public int AccessLevel { get; set; }
        public _AccessLevelTemplate AccessLevelTemplate { get; set; }

        [NotMapped]
        ITerminalDO ISubscriptionDO.TerminalRegistration
        {
            get { return Terminal; }
            set { Terminal = (TerminalDO) value; }
        }
    }
}
