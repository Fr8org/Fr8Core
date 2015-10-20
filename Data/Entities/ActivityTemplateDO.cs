using Data.Entities;
using Data.Interfaces;
using Data.States;
using StructureMap;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;
using System;
using Data.States.Templates;

namespace Data.Entities
{
    public class ActivityTemplateDO : BaseDO
    {
        public ActivityTemplateDO()
        {
            this.ActivityTemplateState = States.ActivityTemplateState.Active;
        }


        public ActivityTemplateDO(string name, string label, string version, int pluginId)
        {
            this.Name = name;
            this.Label = label;
            this.Version = version;
            /* We don't need to validate pluginId because of EF chack ForeignKey and if pluginId doesn't exist in table Plugins then 
             * EF will throw 'System.Data.Entity.Infrastructure.DbUpdateException'  */
            this.PluginID = pluginId;
            this.ActivityTemplateState = States.ActivityTemplateState.Active;
        }

        /// <summary>
        /// Represents a ActionTemplate instance
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        ///<param name="label"></param>
        /// <param name="pluginName">Name of the new PluginDO</param>
        /*<param name="baseEndPoint">New PluginDO base end point</param>*/
        /// <param name="Endpoint">New PluginDO end point</param>
        public ActivityTemplateDO(string name, string version, string pluginName, string endPoint, string label = "")
        {

            this.Name = name;
            this.Label = label;
            this.Version = version;

            this.Plugin = new PluginDO()
            {
                Name = pluginName,
                PluginStatus = PluginStatus.Active,
                Endpoint = endPoint
            };
            this.ActivityTemplateState = States.ActivityTemplateState.Active;
        }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Label { get; set; }

        public string Version { get; set; }

        public string AuthenticationType { get; set; }

        public string ComponentActivities { get; set; }

        [Required]
        [ForeignKey("ActivityTemplateStateTemplate")]
        public int ActivityTemplateState { get; set; }

        public _ActivityTemplateStateTemplate ActivityTemplateStateTemplate { get; set; }

        [ForeignKey("Plugin")]
        public int PluginID { get; set; }
        
        public virtual PluginDO Plugin { get; set; }

        [Required]
        public ActivityCategory Category { get; set; }
    }
}
