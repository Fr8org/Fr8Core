using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;
using Data.Validations;
using FluentValidation;

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
        public string EnvelopeId { get; set; }

        [Required]
        [ForeignKey("ProcessTemplate")]
        public int ProcessTemplateId { get; set; }
        public virtual ProcessTemplateDO ProcessTemplate { get; set; }

        public virtual ICollection<ProcessNodeDO> ProcessNodes { get; set; }
            
        [Required]
        [ForeignKey("ProcessStateTemplate")]
        public int ProcessState { get; set; }
              
        public virtual _ProcessStateTemplate ProcessStateTemplate { get; set; }

        public override void BeforeSave()
        {
            base.BeforeSave();

            ProcessValidator curValidator = new ProcessValidator();
            curValidator.ValidateAndThrow(this);

        }
    }
}