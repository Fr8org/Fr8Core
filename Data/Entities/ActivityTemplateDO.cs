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


        public ActivityTemplateDO(string name, string version, int pluginId)
        {
            this.Name = name;
            this.Version = version;
            this.PluginID = pluginId;
        }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }


        public string AuthenticationType { get; set; }

        public string ComponentActivities { get; set; }

        [ForeignKey("Plugin")]
        public int PluginID { get; set; }
        
        public virtual PluginDO Plugin { get; set; }


    }
}
