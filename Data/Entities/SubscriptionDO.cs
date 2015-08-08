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

        [ForeignKey("DockyardAccount")]
        public string DockyardAccountId { get; set; }
        public virtual DockyardAccountDO DockyardAccount { get; set; }

        [ForeignKey("Plugin")]
        public int PluginId { get; set; }
        public virtual PluginDO Plugin { get; set; }

        [ForeignKey("AccessLevelTemplate")]
        public int AccessLevel { get; set; }
        public _AccessLevelTemplate AccessLevelTemplate { get; set; }

        [NotMapped]
        IPluginRegistrationDO ISubscriptionDO.PluginRegistration
        {
            get { return Plugin; }
            set { Plugin = (PluginDO) value; }
        }
    }
}
