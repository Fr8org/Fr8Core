using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Constants;
using Data.Crates;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Data.Interfaces.Manifests;

namespace Hub.Interfaces
{
    public interface IPlanNode
    {
        List<PlanNodeDO> GetUpstreamActivities(IUnitOfWork uow, PlanNodeDO curActivityDO);

        List<PlanNodeDO> GetDownstreamActivities(IUnitOfWork uow, PlanNodeDO curActivityDO);

        List<T> GetCrateManifestsByDirection<T>(Guid activityId, CrateDirection direction,
            AvailabilityType availability, bool includeCratesFromActivity = true) where T: Manifest;

        //Task Process(Guid curActivityId, ActivityExecutionMode curActionExecutionMode, ContainerDO curContainerDO);

        IncomingCratesDTO GetAvailableData(Guid activityId, CrateDirection direction, AvailabilityType availability);
        IEnumerable<ActivityTemplateDTO> GetAvailableActivities(IUnitOfWork uow, IFr8AccountDO curAccount);
        IEnumerable<ActivityTemplateDTO> GetAvailableActivities(IUnitOfWork uow, Func<ActivityTemplateDO, bool> predicate);

        PlanNodeDO GetNextActivity(PlanNodeDO currentActivity, PlanNodeDO root);
        PlanNodeDO GetNextSibling(PlanNodeDO currentActivity);
        PlanNodeDO GetParent(PlanNodeDO currentActivity);
        PlanNodeDO GetFirstChild(PlanNodeDO currentActivity);
        bool HasChildren(PlanNodeDO currentActivity);

        void Delete(IUnitOfWork uow, PlanNodeDO activity);

        IEnumerable<ActivityTemplateCategoryDTO> GetAvailableActivityGroups();

        IEnumerable<ActivityTemplateDTO> GetSolutions(IUnitOfWork uow);
    }
}