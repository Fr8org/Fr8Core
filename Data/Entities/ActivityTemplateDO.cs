using Data.Entities;
using Data.Interfaces;
using Data.States;
using StructureMap;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System;

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
            /* We don't need to validate pluginId because of EF chack ForeignKey and if pluginId doesn't exist in table Plugins then 
             * EF will throw 'System.Data.Entity.Infrastructure.DbUpdateException'  */
            this.PluginID = pluginId;
        }

        /// <summary>
        /// Represents a ActionTemplate instance
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        /// <param name="pluginName">Name of the new PluginDO</param>
        /*<param name="baseEndPoint">New PluginDO base end point</param>*/
        /// <param name="Endpoint">New PluginDO end point</param>
        public ActivityTemplateDO(string name, string version, string pluginName, string endPoint)
        {

            this.Name = name;
            this.Version = version;

            this.Plugin = new PluginDO()
            {
                Name = pluginName,
                PluginStatus = PluginStatus.Active,
                Endpoint = endPoint
            };
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

        public ActivityCategory Category { get; set; }
    }
}
