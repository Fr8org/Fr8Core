using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;
using Fr8.Infrastructure.Data.States;

namespace Data.Entities
{
    public class ActivityTemplateDO : BaseObject
    {
        public ActivityTemplateDO()
        {
            this.ActivityTemplateState = States.ActivityTemplateState.Active;
            this.Type = ActivityType.Standard;
            this.NeedsAuthentication = false;
        }

        public ActivityTemplateDO(string name, string label, string version, string description, Guid terminalId, ActivityType type = ActivityType.Standard) : this()
        {
            this.Name = name;
            this.Label = label;
            this.Version = version;
            this.Description = description;
            /* We don't need to validate terminalId because of EF chack ForeignKey and if terminalId doesn't exist in table Terminals then 
             * EF will throw 'System.Data.Entity.Infrastructure.DbUpdateException'  */
            this.TerminalId = terminalId;
            this.ActivityTemplateState = States.ActivityTemplateState.Active;
            this.Type = type;
        }

        /// <summary>
        /// Represents a ActionTemplate instance
        /// </summary>
        /// <param name="name"></param>
        /// <param name="version"></param>
        ///<param name="label"></param>
        /// <param name="terminalName">Name of the new TerminalDO</param>
        /// <param name="Endpoint">New TerminalDO end point</param>
        public ActivityTemplateDO(string name, string version,
            string terminalName, string terminalLabel, string endPoint, string label = "", string description = "") : this()
        {

            this.Name = name;
            this.Label = label;
            this.Version = version;
            this.Description = description;

            this.Terminal = new TerminalDO()
            {
                Name = terminalName,
                Label = terminalLabel,
                TerminalStatus = TerminalStatus.Active,
                Endpoint = endPoint
            };
            this.ActivityTemplateState = States.ActivityTemplateState.Active;
        }

        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Label { get; set; }

        public string Tags { get; set; }

        public string Version { get; set; }

        public string Description { get; set; }

        public bool NeedsAuthentication { get; set; }

        [Required]
        [ForeignKey("ActivityTemplateStateTemplate")]
        public int ActivityTemplateState { get; set; }

        public _ActivityTemplateStateTemplate ActivityTemplateStateTemplate { get; set; }

        [ForeignKey("Terminal")]
        public Guid TerminalId { get; set; }

        public virtual TerminalDO Terminal { get; set; }

        [Required]
        public ActivityType Type { get; set; }

        [InverseProperty("ActivityTemplate")]
        public virtual IList<ActivityCategorySetDO> Categories { get; set; }

        public int MinPaneWidth { get; set; }
    }
}
