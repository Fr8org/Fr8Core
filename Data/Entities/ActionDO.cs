using Data.States.Templates;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMapper;
using Data.Wrappers;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace Data.Entities
{
    public class ActionDO : ActivityDO
	{
        public string Name { get; set; }

        public string CrateStorage { get; set; }

        public string FieldMappingSettings { get; set; }

        [ForeignKey("ActionStateTemplate")]
        public int? ActionState { get; set; }

        public virtual _ActionStateTemplate ActionStateTemplate { get; set; }

        public string PayloadMappings { get; set; }

        [ForeignKey("ActionTemplate")]
        public int? ActionTemplateId { get; set; }
        public virtual ActionTemplateDO ActionTemplate { get; set; }

        [NotMapped]
        public bool IsTempId { get; set; }

        public CrateStorageDTO CrateStorageDTO()
        {
            return JsonConvert.DeserializeObject<CrateStorageDTO>(this.CrateStorage);
        }

        public void UpdateCrateStorageDTO(List<CrateDTO> curCratesDTO)
        {
            CrateStorageDTO crateStorageDTO = new CrateStorageDTO();

            if(!String.IsNullOrEmpty(CrateStorage))//if crateStorage is not empty deserialize it
                crateStorageDTO = CrateStorageDTO();

            crateStorageDTO.CratesDTO.AddRange(curCratesDTO);

            this.CrateStorage = JsonConvert.SerializeObject(crateStorageDTO);
        }
    }
}