using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States;
using Data.States.Templates;
using Data.Validations;

using FluentValidation;
namespace Data.Entities
{
    public class ProcessNodeTemplateDO : BaseDO
    {
        public ProcessNodeTemplateDO()
        {
            this.Criteria = new List<CriteriaDO>();
            this.ActionLists = new List<ActionListDO>();
            this.ProcessNode = new List<ProcessNodeDO>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        [ForeignKey("ProcessTemplate")]
        public int? ParentTemplateId { get; set; }

        public virtual ProcessTemplateDO ProcessTemplate { get; set; }

        /// <summary>
        /// this is a JSON structure that is a array of key-value pairs that represent possible transitions. Example:
        ///[{'TransitionKey':'true','ProcessNodeId':'234kljdf'},{'TransitionKey':'false','ProcessNodeId':'dfgkjfg'}]. In this case the values are Id's of other ProcessNodes.
        /// </summary>
        public string NodeTransitions { get; set; }

        public virtual List<CriteriaDO> Criteria { get; set; }

        public virtual List<ActionListDO> ActionLists { get; set; }

        public virtual List<ProcessNodeDO> ProcessNode { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
        public override void BeforeSave()
        {
            base.BeforeSave();

            // TODO: commented out.
            // TODO: Currently crashes on ProcessTemplate creation.
            //       When ProcessTemplate is created, empty StartProcessNodeTemplate is created and assigned to ProcessTemplate.
            //       Need to create another issue to fix that.
            // ProcessNodeTemplatetValidator curValidator = new ProcessNodeTemplatetValidator();
            // curValidator.ValidateAndThrow(this);
        }
    }
}