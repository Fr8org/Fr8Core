using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using System.Threading.Tasks;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;

namespace Hub.Interfaces
{
    public interface IPlan
    {
        PlanResultDTO GetForUser(IUnitOfWork uow, Fr8AccountDO account, PlanQueryDTO planQueryDTO, bool isAdmin);
        IList<PlanDO> GetByName(IUnitOfWork uow, Fr8AccountDO account, string name, PlanVisibility visibility);
        void CreateOrUpdate(IUnitOfWork uow, PlanDO submittedPlan);
        PlanDO Create(IUnitOfWork uow, string name, string category = "");
        PlanDO GetFullPlan(IUnitOfWork uow, Guid id);
        Task Delete(Guid id);

        IList<PlanDO> GetMatchingPlans(string userId, EventReportCM curEventReport);
        Task<ActivateActivitiesDTO> Activate(Guid planId, bool planBuilderActivate);
        Task Deactivate(Guid curPlanId);

        PlanDO GetPlanByActivityId(IUnitOfWork uow, Guid planActivityId);
        List<PlanDO> MatchEvents(List<PlanDO> curPlans, EventReportCM curEventReport);
        bool IsMonitoringPlan(IUnitOfWork uow, PlanDO planDo);

        void Enqueue(Guid curPlanId, params Crate[] curEventReport);
        Task<ContainerDO> Run(IUnitOfWork uow, PlanDO plan, Crate[] curPayload);
        PlanDO Clone(Guid planId);
    }
}
