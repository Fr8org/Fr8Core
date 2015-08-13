using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class ActionRegistrationDO : BaseDO
    {
        public ActionRegistrationDO() { }

        public ActionRegistrationDO(string actionType, string parentPluginRegistration, string version)
        {
            this.ActionType = actionType;
            this.ParentPluginRegistration = parentPluginRegistration;
            this.Version = version;
        }

        [Key]
        public int Id { get; set; }

        public string ActionType { get; set; }

        public string Version { get; set; }

        public string ParentPluginRegistration { get; set; }
    }
}
