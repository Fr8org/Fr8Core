using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class ActionRegistrationDO : BaseDO
    {
        [Key]
        public int Id { get; set; }

        public string ActionType { get; set; }

        public string Version { get; set; }

        public string ParentPluginRegistration { get; set; }
    }
}
