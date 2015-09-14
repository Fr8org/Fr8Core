using Data.States;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class ActivityTemplateDO : BaseDO
    {
        public ActivityTemplateDO()
        {

        }

        public ActivityTemplateDO(string name, string defaultEndPoint, string version)
        {
            this.Name = name;
            this.DefaultEndPoint = defaultEndPoint;
            this.Version = version;
            this.Plugin = new PluginDO()
            {
                Name = defaultEndPoint,
                PluginStatus = PluginStatus.Active
            };
        }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }

        public string DefaultEndPoint { get; set; }

        public string AuthenticationType { get; set; }

        public string ComponentActivities { get; set; }

        [ForeignKey("Plugin")]
        public int? PluginID { get; set; }
        public virtual PluginDO Plugin { get; set; }
    }
}
