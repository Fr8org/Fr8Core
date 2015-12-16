using Data.States;
using Data.States.Templates;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMapper;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace Data.Entities
{
    public class ActionDO : RouteNodeDO
	{
        public string Name { get; set; }

        public string CrateStorage { get; set; }
        public string Label { get; set; }

        [ForeignKey("ActivityTemplate")]
        public int? ActivityTemplateId { get; set; }

        public virtual ActivityTemplateDO ActivityTemplate { get; set; }

        [NotMapped]
        public bool IsTempId { get; set; }

        [NotMapped]
        public string ExplicitData { get; set; }

        public string currentView { get; set; }

        public override RouteNodeDO Clone()
        {
            return new ActionDO()
            {
                Ordering = this.Ordering,
                Name = this.Name,
                CrateStorage = this.CrateStorage,
                Label = this.Label,
                ActivityTemplateId = this.ActivityTemplateId
            };
        }

//        public CrateStorageDTO CrateStorageDTO()
//        {
//            return JsonConvert.DeserializeObject<CrateStorageDTO>(this.CrateStorage);
//        }
//
//        public void UpdateCrateStorageDTO(List<CrateDTO> curCratesDTO)
//        {
//            CrateStorageDTO crateStorageDTO = new CrateStorageDTO();
//
//            if(!String.IsNullOrEmpty(CrateStorage))//if crateStorage is not empty deserialize it
//                crateStorageDTO = CrateStorageDTO();
//
//            crateStorageDTO.CrateDTO.AddRange(curCratesDTO);
//
//            this.CrateStorage = JsonConvert.SerializeObject(crateStorageDTO);
//        }
    }
}