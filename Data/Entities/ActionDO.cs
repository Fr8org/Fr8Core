using Data.States.Templates;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMapper;
using Data.Wrappers;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;

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

        [ForeignKey("ActionStateTemplate")]
        public int? ActionState { get; set; }

        public virtual _ActionStateTemplate ActionStateTemplate { get; set; }

        public string PayloadMappings { get; set; }

        [ForeignKey("ActionTemplate")]
        public int? ActionTemplateId { get; set; }
        public virtual ActivityTemplateDO ActionTemplate { get; set; }

        [NotMapped]
        public bool IsTempId { get; set; }

        public CrateStorageDTO CrateStorageDTO()
        {
                return JsonConvert.DeserializeObject<CrateStorageDTO>(this.ConfigurationStore);

        }
    }
}