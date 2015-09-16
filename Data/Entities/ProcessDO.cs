using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Interfaces.DataTransferObjects;
using Data.States.Templates;
using Data.Validations;
using FluentValidation;
using Data.Interfaces.DataTransferObjects;
using Newtonsoft.Json;
using System;

namespace Data.Entities
{
    public class ProcessDO : BaseDO
    {
        public ProcessDO()
        {
            ProcessNodes = new List<ProcessNodeDO>();
        }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string DockyardAccountId { get; set; }

        [Required]
        [ForeignKey("ProcessTemplate")]
        public int ProcessTemplateId { get; set; }
        public virtual ProcessTemplateDO ProcessTemplate { get; set; }

        public virtual ICollection<ProcessNodeDO> ProcessNodes { get; set; }
            
        [Required]
        [ForeignKey("ProcessStateTemplate")]
        public int ProcessState { get; set; }
              
        public virtual _ProcessStateTemplate ProcessStateTemplate { get; set; }

        [ForeignKey("CurrentActivity")]
        public int? CurrentActivityId { get; set; }
        public virtual ActivityDO CurrentActivity { get; set; }

        [ForeignKey("NextActivity")]
        public int? NextActivityId { get; set; }
        public virtual ActivityDO NextActivity { get; set; }

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

            ProcessValidator curValidator = new ProcessValidator();
            curValidator.ValidateAndThrow(this);

        }
    }
}