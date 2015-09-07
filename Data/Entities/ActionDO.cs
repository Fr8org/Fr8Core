using Data.States.Templates;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Wrappers;
using Data.Interfaces;

namespace Data.Entities
{
    public class ActionDO : ActivityDO /*, IActionListChild*/
	{
        public string Name { get; set; }

        // [ForeignKey("ParentActionList")]
        // public int? ParentActionListId { get; set; }
        // public virtual ActionListDO ParentActionList { get; set; }

        public string ConfigurationStore { get; set; }

        public string FieldMappingSettings { get; set; }

        // TODO: We should probably remove this property.
        // TODO: We can access ParentPluginRegistration via ActionDO.ActionTemplate.ParentPluginRegistration.
        public string ParentPluginRegistration { get; set; }

        [ForeignKey("ActionStateTemplate")]
        public int? ActionState { get; set; }

        public virtual _ActionStateTemplate ActionStateTemplate { get; set; }

        public string PayloadMappings { get; set; }

        [ForeignKey("ActionTemplate")]
        public int? ActionTemplateId { get; set; }
        public virtual ActionTemplateDO ActionTemplate { get; set; }

        [NotMapped]
        public bool IsTempId { get; set; }
	}
}