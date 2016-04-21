using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Data.Crates;
using Data.Entities;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
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
            if (_securityServices.AuthorizeActivity(Permission.EditObject, planId.ToString()))
            {
                return _target.Activate(planId, planBuilderActivate);
            }
            else
            {
                throw new HttpException(401, "You are not authorized to perform this activity!");
            }
        }

        public Task<PlanDO> Clone(Guid planId)
        {
            if (_securityServices.AuthorizeActivity(Permission.ReadObject, planId.ToString()))
            {
                return _target.Clone(planId);
            }
            else
            {
                throw new HttpException(401, "You are not authorized to perform this activity!");
            }
        }

        public Task<ContainerDO> Continue(Guid containerId)
        {
            if (_securityServices.AuthorizeActivity(Permission.EditObject, containerId.ToString()))
            {
                return _target.Continue(containerId);
            }
            else
            {
                throw new HttpException(401, "You are not authorized to perform this activity!");
            }
        }

        public PlanDO Copy(IUnitOfWork uow, PlanDO curPlanDO, string name)
        {
            if (_securityServices.AuthorizeActivity(Permission.ReadObject, curPlanDO.Id.ToString()))
            {
                return _target.Copy(uow, curPlanDO, name);
            }
            else
            {
                throw new HttpException(401, "You are not authorized to perform this activity!");
            }
        }

        public ContainerDO Create(IUnitOfWork uow, Guid planId, params Crate[] curPayload)
        {
            if (_securityServices.AuthorizeActivity(Permission.EditObject, planId.ToString()))
            {
                return _target.Create(uow, planId, curPayload);
            }
            else
            {
                throw new HttpException(401, "You are not authorized to perform this activity!");
            }
        }

        public PlanDO Create(IUnitOfWork uow, string name, string category = "")
        {
            return _target.Create(uow, name, category);
        }

        public void CreateOrUpdate(IUnitOfWork uow, PlanDO submittedPlan, bool withTemplate)
        {
            _target.CreateOrUpdate(uow, submittedPlan, withTemplate);
        }

        public Task<string> Deactivate(Guid curPlanId)
        {
            if (_securityServices.AuthorizeActivity(Permission.EditObject, curPlanId.ToString()))
            {
                return _target.Deactivate(curPlanId);
            }
            else
            {
                throw new HttpException(401, "You are not authorized to perform this activity!");
            }
        }

        public void Delete(IUnitOfWork uow, Guid id)
        {
            if (_securityServices.AuthorizeActivity(Permission.DeleteObject, id.ToString()))
            {
                _target.Delete(uow, id);
            }
            else
            {
                throw new HttpException(401, "You are not authorized to perform this activity!");
            }
        }

        public void Enqueue(List<PlanDO> curPlans, params Crate[] curEventReport)
        {
            _target.Enqueue(curPlans, curEventReport);
        }

        public void Enqueue(Guid curPlanId, params Crate[] curEventReport)
        {
            if (_securityServices.AuthorizeActivity(Permission.EditObject, curPlanId.ToString()))
            {
                _target.Enqueue(curPlanId, curEventReport);
            }
            else
            {
                throw new HttpException(401, "You are not authorized to perform this activity!");
            }
        }

        public IList<PlanDO> GetByName(IUnitOfWork uow, Fr8AccountDO account, string name, PlanVisibility visibility)
        {
            return _target.GetByName(uow, account, name, visibility);
        }

        public IList<PlanDO> GetForUser(IUnitOfWork uow, Fr8AccountDO account, bool isAdmin, Guid? id = default(Guid?), int? status = default(int?), string category = "")
        {
            return _target.GetForUser(uow, account, isAdmin, id ,status,category);
        }

        public IList<PlanDO> GetMatchingPlans(string userId, EventReportCM curEventReport)
        {
            return _target.GetMatchingPlans(userId, curEventReport);
        }

        public PlanDO GetPlanByActivityId(IUnitOfWork uow, Guid planActivityId)
        {
            if (_securityServices.AuthorizeActivity(Permission.ReadObject, planActivityId.ToString()))
            {
                return _target.GetPlanByActivityId(uow, planActivityId);
            }
            else
            {
                throw new HttpException(401, "You are not authorized to perform this activity!");
            }
        }

        public List<PlanDO> MatchEvents(List<PlanDO> curPlans, EventReportCM curEventReport)
        {
            return _target.MatchEvents(curPlans, curEventReport);
        }

        public Task<ContainerDO> Run(Guid planId, params Crate[] curPayload)
        {
            if (_securityServices.AuthorizeActivity(Permission.EditObject, planId.ToString()))
            {
                return _target.Run(planId, curPayload);
            }
            else
            {
                throw new HttpException(401, "You are not authorized to perform this activity!");
            }
        }

        public Task<ContainerDO> Run(PlanDO curPlan, params Crate[] curPayload)
        {
            if (_securityServices.AuthorizeActivity(Permission.EditObject, curPlan.Id.ToString()))
            {
                return _target.Run(curPlan, curPayload);
            }
            else
            {
                throw new HttpException(401, "You are not authorized to perform this activity!");
            }
        }

        public Task<ContainerDO> Run(IUnitOfWork uow, PlanDO curPlan, params Crate[] curPayload)
        {
            if (_securityServices.AuthorizeActivity(Permission.EditObject, curPlan.Id.ToString()))
            {
                return _target.Run(uow, curPlan, curPayload);
            }
            else
            {
                throw new HttpException(401, "You are not authorized to perform this activity!");
            }
        }
    }
}
