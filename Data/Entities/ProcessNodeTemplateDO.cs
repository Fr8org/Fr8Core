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
    public class ProcessNodeTemplateDO : ActivityDO
    {
        public ProcessNodeTemplateDO()
        {
           // this.Criteria = new List<CriteriaDO>();
           // this.ProcessNode = new List<ProcessNodeDO>();
        }

        public ProcessNodeTemplateDO(bool startingProcessNodeTemplate)
        {
            this.StartingProcessNodeTemplate = true;
           // this.Criteria = new List<CriteriaDO>();
           // this.ProcessNode = new List<ProcessNodeDO>();
        }

        public string Name { get; set; }

        public bool StartingProcessNodeTemplate { get; set; } 

        /// <summary>
        /// this is a JSON structure that is a array of key-value pairs that represent possible transitions. Example:
        ///[{'TransitionKey':'true','ProcessNodeId':'234kljdf'},{'TransitionKey':'false','ProcessNodeId':'dfgkjfg'}]. In this case the values are Id's of other ProcessNodes.
        /// </summary>
        public string NodeTransitions { get; set; }

        public virtual List<CriteriaDO> Criteria { get; set; }

        public virtual List<ProcessNodeDO> ProcessNode { get; set; }

        [NotMapped]
        public RouteDO Route
        {
            get { return (RouteDO) ParentActivity; }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public override void BeforeSave()
        {
            base.BeforeSave();

            // TODO: commented out.
            // TODO: Currently crashes on Route creation.
            //       When Route is created, empty StartProcessNodeTemplate is created and assigned to Route.
            //       Need to create another issue to fix that.
            // ProcessNodeTemplatetValidator curValidator = new ProcessNodeTemplatetValidator();
            // curValidator.ValidateAndThrow(this);
        }
    }
}