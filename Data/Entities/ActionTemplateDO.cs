using Data.States;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class ActionTemplateDO : BaseDO
    {
        public ActionTemplateDO() {

        }


        public ActionTemplateDO(string name, string version, int pluginId)
        {
            this.Name = name;
            this.Version = version;
            this.PluginID = pluginId;
        }

        /// <summary>
        /// Represents a ActionTemplate instance
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="pluginName">Name of the new PluginDO</param>
        /// <param name="baseEndPoint">New PluginDO base end point</param>
        /// <param name="Endpoint">New PluginDO end point</param>
        public ActionTemplateDO(string name, string version, string pluginName, string baseEndpoint, string endPoint)
        {
            this.Name = name;
            //this.DefaultEndPoint = defaultEndPoint;
            this.Version = version;
            this.Plugin = new PluginDO()
            {
                Name = name,
                PluginStatus = PluginStatus.Active,
                BaseEndPoint = baseEndpoint,
                Endpoint = endPoint
            };
        }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }


        public string AuthenticationType { get; set; }

        [ForeignKey("Plugin")]
        public int PluginID { get; set; }
        
        public virtual PluginDO Plugin { get; set; }

        public string ActionProcessor { get; set; }
    }
}
