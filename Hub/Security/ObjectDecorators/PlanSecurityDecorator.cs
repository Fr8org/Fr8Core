using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.States;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Hub.Interfaces;

namespace Hub.Security.ObjectDecorators
{
    public class PlanSecurityDecorator : IPlan
    {
        private readonly IPlan _target;
        private readonly ISecurityServices _securityServices;

        public PlanSecurityDecorator(IPlan target, ISecurityServices securityServices)
        {
            _securityServices = securityServices;
            _target = target;
        }

        public Task<ActivateActivitiesDTO> Activate(Guid planId, bool planBuilderActivate)
        {
            if (_securityServices.AuthorizeActivity(PermissionType.EditObject, planId, nameof(PlanNodeDO)))
            {
                return _target.Activate(planId, planBuilderActivate);
            }
            else
            {
                throw new HttpException(403, "You are not authorized to perform this activity!");
            }
        }

        public PlanDO Clone(Guid planId)
        {
            if (_securityServices.AuthorizeActivity(PermissionType.ReadObject, planId, nameof(PlanNodeDO)))
            {
                return _target.Clone(planId);
            }
            else
            {
                throw new HttpException(403, "You are not authorized to perform this activity!");
            }
        }

        public bool IsMonitoringPlan(IUnitOfWork uow, PlanDO planDo)
        {
            return _target.IsMonitoringPlan(uow, planDo);
        }

        public bool IsPlanActiveOrExecuting(Guid planNodeId)
        {
            return _target.IsPlanActiveOrExecuting(planNodeId);
        }

        public int? GetPlanState(IUnitOfWork uow, Guid planNodeId)
        {
            return _target.GetPlanState(uow, planNodeId);
        }

        public PlanDO Create(IUnitOfWork uow, string name, string category = "", string ownerId = "", PlanVisibility visibility = PlanVisibility.Standard)
        {
            //TODO: probably worth add a check that only admin can create plan with specific owner Id and internal plan visibility
            return _target.Create(uow, name, category, ownerId, visibility);
        }

        public PlanDO GetFullPlan(IUnitOfWork uow, Guid planId)
        {
            if (_securityServices.AuthorizeActivity(PermissionType.ReadObject, planId, nameof(PlanNodeDO)))
            {
                return _target.GetFullPlan(uow, planId);
            }
            else
            {
                throw new HttpException(403, "You are not authorized to perform this activity!");
            }
        }

        public void CreateOrUpdate(IUnitOfWork uow, PlanDO submittedPlan)
        {
            _target.CreateOrUpdate(uow, submittedPlan);
        }

        public Task Deactivate(Guid curPlanId)
        {
            if (_securityServices.AuthorizeActivity(PermissionType.EditObject, curPlanId, nameof(PlanNodeDO)))
            {
                return _target.Deactivate(curPlanId);
            }
           
            throw new HttpException(403, "You are not authorized to perform this activity!");
        }

        public async Task Delete(Guid id)
        {
            if (_securityServices.AuthorizeActivity(PermissionType.DeleteObject, id, nameof(PlanNodeDO)))
            {
               await _target.Delete(id);
            }
            else
            {
                throw new HttpException(403, "You are not authorized to perform this activity!");
            }
        }
        
        public void Enqueue(Guid curPlanId, params Crate[] curEventReport)
        {
            if (_securityServices.AuthorizeActivity(PermissionType.EditObject, curPlanId, nameof(PlanNodeDO)))
            {
                _target.Enqueue(curPlanId, curEventReport);
            }
            else
            {
                throw new HttpException(403, "You are not authorized to perform this activity!");
            }
        }

        public IList<PlanDO> GetByName(IUnitOfWork uow, Fr8AccountDO account, string name, PlanVisibility visibility)
        {
            return _target.GetByName(uow, account, name, visibility);
        }

        public PlanResultDTO GetForUser(IUnitOfWork uow, Fr8AccountDO account, PlanQueryDTO planQueryDTO, bool isAdmin)
        {
            return _target.GetForUser(uow, account, planQueryDTO, isAdmin);
        }

        public IList<PlanDO> GetMatchingPlans(string userId, EventReportCM curEventReport)
        {
            return _target.GetMatchingPlans(userId, curEventReport);
        }

        public PlanDO GetPlanByActivityId(IUnitOfWork uow, Guid planActivityId)
        {
            if (_securityServices.AuthorizeActivity(PermissionType.ReadObject, planActivityId, nameof(PlanNodeDO)))
            {
                return _target.GetPlanByActivityId(uow, planActivityId);
            }
            else
            {
                throw new HttpException(403, "You are not authorized to perform this activity!");
            }
        }

        public List<PlanDO> MatchEvents(List<PlanDO> curPlans, EventReportCM curEventReport)
        {
            return _target.MatchEvents(curPlans, curEventReport);
        }
        
        public Task<ContainerDTO> Run(Guid planId, Crate[] payload, Guid? containerId)
        {
            if (_securityServices.AuthorizeActivity(PermissionType.RunObject, planId, nameof(PlanNodeDO)))
            {
                return _target.Run(planId, payload, containerId);
            }

            throw new HttpException(403, "You are not authorized to perform this activity!");
        }
    }
}
