using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces.DataTransferObjects;
using Data.States.Templates;
using Data.Validations;
using FluentValidation;
using Newtonsoft.Json;
using System;

namespace Data.Entities
{
    public class ContainerDO : BaseDO
    {
        public ContainerDO()
        {
            ProcessNodes = new List<ProcessNodeDO>();
        }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Fr8AccountId { get; set; }

        [Required]
        [ForeignKey("Route")]
        public int RouteId { get; set; }
        public virtual RouteDO Route { get; set; }

        public virtual ICollection<ProcessNodeDO> ProcessNodes { get; set; }
            
        [Required]
        [ForeignKey("ContainerStateTemplate")]
        public int ContainerState { get; set; }
              
        public virtual _ContainerStateTemplate ContainerStateTemplate { get; set; }

        [ForeignKey("CurrentRouteNode")]
        public int? CurrentRouteNodeId { get; set; }
        public virtual RouteNodeDO CurrentRouteNode { get; set; }

        [ForeignKey("NextRouteNode")]
        public int? NextRouteNodeId { get; set; }
        public virtual RouteNodeDO NextRouteNode { get; set; }

        public string CrateStorage { get; set; }

        public CrateStorageDTO CrateStorageDTO()
        {
            return JsonConvert.DeserializeObject<CrateStorageDTO>(this.CrateStorage);
        }

        public void UpdateCrateStorageDTO(List<CrateDTO> curCratesDTO)
        {
            CrateStorageDTO crateStorageDTO = new CrateStorageDTO();

            if (!string.IsNullOrEmpty(CrateStorage))
            {
                crateStorageDTO = this.CrateStorageDTO();
            }

            crateStorageDTO.CrateDTO.AddRange(curCratesDTO);

            this.CrateStorage = JsonConvert.SerializeObject(crateStorageDTO);
        }

        public override void BeforeSave()
        {
            base.BeforeSave();

            ContainerValidator curValidator = new ContainerValidator();
            curValidator.ValidateAndThrow(this);

        }
    }
}