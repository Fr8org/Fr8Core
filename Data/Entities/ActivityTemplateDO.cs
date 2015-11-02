using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Data.States.Templates;
using StructureMap;

namespace Data.Entities
{
    public class ActivityTemplateDO : BaseDO
    {
        public ActivityTemplateDO()
        {
            this.AuthenticationType = States.AuthenticationType.None;
        }

        public ActivityTemplateDO(string name, string label, string version, int terminalId) : this()
        {
            this.Name = name;
            this.Label = label;
            this.Version = version;
            /* We don't need to validate pluginId because of EF chack ForeignKey and if pluginId doesn't exist in table Plugins then 
             * EF will throw 'System.Data.Entity.Infrastructure.DbUpdateException'  */
            this.TerminalID = terminalId;
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
        public ActivityTemplateDO(string name, string version,
            string pluginName, string endPoint, string label = "") : this()
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
        }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Label { get; set; }

        public string Tags { get; set; }

        public string Version { get; set; }

        [Required]
        [ForeignKey("AuthenticationTypeTemplate")]
        public int AuthenticationType { get; set; }

        public virtual _AuthenticationTypeTemplate AuthenticationTypeTemplate { get; set; }

        public string ComponentActivities { get; set; }

        [ForeignKey("Plugin")]
        public int TerminalID { get; set; }
        
        public virtual PluginDO Plugin { get; set; }

        [Required]
        public ActivityCategory Category { get; set; }

        public int MinPaneWidth { get; set; }

		public int? WebServiceId { get; set; }

		public virtual WebServiceDO WebService { get; set; }
    }
}
