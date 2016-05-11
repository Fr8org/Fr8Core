using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using System.Threading.Tasks;
using Data.Crates;
using Data.States;

namespace Hub.Interfaces
{
    public interface IPlan
    {
        PlanResultDTO GetForUser(IUnitOfWork uow, Fr8AccountDO account, PlanQueryDTO planQueryDTO, bool isAdmin);
        IList<PlanDO> GetByName(IUnitOfWork uow, Fr8AccountDO account, string name, PlanVisibility visibility);
        void CreateOrUpdate(IUnitOfWork uow, PlanDO submittedPlan, bool withTemplate);
        PlanDO Create(IUnitOfWork uow, string name, string category = "");
        Task Delete(IUnitOfWork uow, Guid id);

        IList<PlanDO> GetMatchingPlans(string userId, EventReportCM curEventReport);
        Task<ActivateActivitiesDTO> Activate(Guid planId, bool planBuilderActivate);
        Task<string> Deactivate(Guid curPlanId);

        PlanDO GetPlanByActivityId(IUnitOfWork uow, Guid planActivityId);
        List<PlanDO> MatchEvents(List<PlanDO> curPlans, EventReportCM curEventReport);

        PlanDO Copy(IUnitOfWork uow, PlanDO curPlanDO, string name);

        void Enqueue(Guid curPlanId, params Crate[] curEventReport);
        void Enqueue(List<PlanDO> curPlans, params Crate[] curEventReport);
        ContainerDO Create(IUnitOfWork uow, Guid planId, params Crate[] curPayload);
        Task<ContainerDO> Run(Guid planId, Crate[] curPayload);
        Task<ContainerDO> Continue(Guid containerId);
        Task<PlanDO> Clone(Guid planId);
    }
}
