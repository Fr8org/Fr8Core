using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using System.Threading.Tasks;
using Data.Crates;

namespace Hub.Interfaces
{
    public interface IPlan
    {
        IList<PlanDO> GetForUser(IUnitOfWork uow, Fr8AccountDO account, bool isAdmin, Guid? id = null, int? status = null);
        IList<PlanDO> GetByName(IUnitOfWork uow, Fr8AccountDO account, string name);
        void CreateOrUpdate(IUnitOfWork uow, PlanDO ptdo, bool withTemplate);
        PlanDO Create(IUnitOfWork uow, string name);
        void Delete(IUnitOfWork uow, Guid id);
        RouteNodeDO GetInitialActivity(IUnitOfWork uow, PlanDO curPlan);
        
        IList<PlanDO> GetMatchingPlans(string userId, EventReportCM curEventReport);
        RouteNodeDO GetFirstActivity(Guid curPlanId);
        Task<ActivateActionsDTO> Activate(Guid planId, bool routeBuilderActivate);
        Task<string> Deactivate(Guid curPlanId);
        
        PlanDO GetPlan(ActivityDO activity);
        //  ActionListDO GetActionList(IUnitOfWork uow, int id);
        List<PlanDO> MatchEvents(List<PlanDO> curPlans, EventReportCM curEventReport);

        PlanDO Copy(IUnitOfWork uow, PlanDO curPlanDO, string name);


        ContainerDO Create(IUnitOfWork uow, Guid planId, Crate curEvent);
        Task<ContainerDO> Run(PlanDO curPlan, Crate curEvent);
        Task<ContainerDO> Continue(Guid containerId);
    }
}    
