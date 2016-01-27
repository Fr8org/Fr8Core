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
    public class SubrouteDO : RouteNodeDO
    {
        public SubrouteDO()
        {
           // this.Criteria = new List<CriteriaDO>();
           // this.ProcessNode = new List<ProcessNodeDO>();
        }

        public SubrouteDO(bool startingSubroute)
        {
            this.StartingSubroute = true;
           // this.Criteria = new List<CriteriaDO>();
           // this.ProcessNode = new List<ProcessNodeDO>();
        }

        public string Name { get; set; }

        public bool StartingSubroute { get; set; } 

        /// <summary>
        /// this is a JSON structure that is a array of key-value pairs that represent possible transitions. Example:
        ///[{'TransitionKey':'true','ProcessNodeId':'234kljdf'},{'TransitionKey':'false','ProcessNodeId':'dfgkjfg'}]. In this case the values are Id's of other ProcessNodes.
        /// </summary>
        public string NodeTransitions { get; set; }

        public virtual List<CriteriaDO> Criteria { get; set; }

        public virtual List<ProcessNodeDO> ProcessNode { get; set; }

        [NotMapped]
        public PlanDO Plan
        {
            get { return (PlanDO) ParentRouteNode; }
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
            //       When Route is created, empty StartSubroute is created and assigned to Route.
            //       Need to create another issue to fix that.
            // SubroutetValidator curValidator = new SubroutetValidator();
            // curValidator.ValidateAndThrow(this);
        }

        public override RouteNodeDO Clone()
        {
            return new SubrouteDO()
            {
                Ordering = this.Ordering,
                Name = this.Name,
                StartingSubroute = this.StartingSubroute
            };
        }
    }
}