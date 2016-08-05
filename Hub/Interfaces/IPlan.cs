using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using System.Threading.Tasks;
using Data.States;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;

namespace Hub.Interfaces
{
    public interface IPlan
    {
        PlanResultDTO GetForUser(IUnitOfWork uow, Fr8AccountDO account, PlanQueryDTO planQueryDTO, bool isAdmin);
        IList<PlanDO> GetByName(IUnitOfWork uow, Fr8AccountDO account, string name, PlanVisibility visibility);
        void CreateOrUpdate(IUnitOfWork uow, PlanDO submittedPlan);
        PlanDO Create(IUnitOfWork uow, string name, string category = "", string ownerId = "", PlanVisibility visibility = PlanVisibility.Standard);
        PlanDO GetFullPlan(IUnitOfWork uow, Guid id);
        Task Delete(Guid id);

        IList<PlanDO> GetMatchingPlans(string userId, EventReportCM curEventReport);
        Task<ActivateActivitiesDTO> Activate(Guid planId, bool planBuilderActivate);
        Task Deactivate(Guid curPlanId);

        PlanDO GetPlanByActivityId(IUnitOfWork uow, Guid planActivityId);
        List<PlanDO> MatchEvents(List<PlanDO> curPlans, EventReportCM curEventReport);
        bool IsMonitoringPlan(IUnitOfWork uow, PlanDO planDo);
        bool IsPlanActiveOrExecuting(Guid planNodeId);
        int? GetPlanState(IUnitOfWork uow, Guid planNodeId);
        void Enqueue(Guid curPlanId, params Crate[] curEventReport);
        Task<ContainerDTO> Run(Guid planId, Crate[] payload, Guid? containerId);
        PlanDO Clone(Guid planId);
    }
}
