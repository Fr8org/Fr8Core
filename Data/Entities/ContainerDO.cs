using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.States.Templates;
using Data.Validations;
using FluentValidation;
using System;
using System.Data.Entity.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.States;
using StructureMap;


namespace Data.Entities
{
    public class ContainerDO : BaseObject
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }

        //public string Fr8AccountId { get; set; }

        [Required]
        [ForeignKey("Plan")]
        public Guid PlanId { get; set; }
        public virtual PlanDO Plan { get; set; }

        [Required]
        [ForeignKey("ContainerStateTemplate")]
        public int State { get; set; }

        public virtual _ContainerStateTemplate ContainerStateTemplate { get; set; }

        [ForeignKey("CurrentPlanNode")]
        public Guid? CurrentActivityId { get; set; }
        public virtual PlanNodeDO CurrentPlanNode { get; set; }

        [ForeignKey("NextRouteNode")]
        public Guid? NextActivityId { get; set; }
        public virtual PlanNodeDO NextRouteNode { get; set; }

        public string CrateStorage { get; set; }

        public override void BeforeSave()
        {
            base.BeforeSave();

            ContainerValidator curValidator = new ContainerValidator();
            curValidator.ValidateAndThrow(this);

        }
        public override void OnModify(DbPropertyValues originalValues, DbPropertyValues currentValues)
        {
            base.OnModify(originalValues, currentValues);
        }

        public override void AfterCreate()
        {
            base.AfterCreate();

            var securityService = ObjectFactory.GetInstance<ISecurityServices>();
            securityService.SetDefaultRecordBasedSecurityForObject(Roles.OwnerOfCurrentObject, Id, GetType().Name);
        }
    }
}