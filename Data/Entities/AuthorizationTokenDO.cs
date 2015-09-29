using Data.States;
using Data.States.Templates;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class AuthorizationTokenDO : BaseDO
    {
        public AuthorizationTokenDO()
        {
            Id = Guid.NewGuid();
            Plugin = new PluginDO() { Name = "", Version = "1", PluginStatus = PluginStatus.Active };
        }

        public Guid Id { get; set; }
        public String Token { get; set; }
        public String RedirectURL { get; set; }
        public String SegmentTrackingEventName { get; set; }
        public String SegmentTrackingProperties { get; set; }
        public DateTime ExpiresAt { get; set; }

        public String ExternalAccountId { get; set; }


        [ForeignKey("UserDO")]
        public String UserID { get; set; }
        public virtual DockyardAccountDO UserDO { get; set; }

        [ForeignKey("Plugin")]
        public int PluginID { get; set; }
        public virtual PluginDO Plugin { get; set; }

        [ForeignKey("AuthorizationTokenStateTemplate")]
        public int? AuthorizationTokenState { get; set; }

        public virtual _AuthorizationTokenStateTemplate AuthorizationTokenStateTemplate { get; set; }

    }
}
