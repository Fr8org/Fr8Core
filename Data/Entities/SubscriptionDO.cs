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
    public class SubscriptionDO : BaseDO, ISubscriptionDO
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Account")]
        public string AccountId { get; set; }
        public virtual DockyardAccountDO Account { get; set; }

        [ForeignKey("PluginRegistration")]
        public int PluginRegistrationId { get; set; }
        public virtual PluginRegistrationDO PluginRegistration { get; set; }

        [ForeignKey("AccessLevelTemplate")]
        public int AccessLevel { get; set; }
        public _AccessLevelTemplate AccessLevelTemplate { get; set; }

        [NotMapped]
        IPluginRegistrationDO ISubscriptionDO.PluginRegistration
        {
            get { return PluginRegistration; }
            set { PluginRegistration = (PluginRegistrationDO) value; }
        }
    }
}
