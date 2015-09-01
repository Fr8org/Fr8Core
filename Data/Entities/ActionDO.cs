using Data.States.Templates;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Wrappers;

namespace Data.Entities
{
    public class ActionDO : ActivityDO
	{
        public string Name { get; set; }

        [ForeignKey("ActionList")]
        public int? ActionListId { get; set; }
        public virtual ActionListDO ActionList { get; set; }

        public string ConfigurationSettings { get; set; }

        public string FieldMappingSettings { get; set; }

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