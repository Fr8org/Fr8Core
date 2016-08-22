using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;

namespace Hub.Interfaces
{
    public interface IPlanNode
    {
        List<PlanNodeDO> GetUpstreamActivities(IUnitOfWork uow, PlanNodeDO curActivityDO);

        List<PlanNodeDO> GetDownstreamActivities(IUnitOfWork uow, PlanNodeDO curActivityDO);

        List<CrateDescriptionCM> GetCrateManifestsByDirection(Guid activityId, CrateDirection direction,
            AvailabilityType availability, bool includeCratesFromActivity = true);

        IncomingCratesDTO GetIncomingData(Guid activityId, CrateDirection direction, AvailabilityType availability);
        IEnumerable<ActivityTemplateDTO> GetAvailableActivities(IUnitOfWork uow, IFr8AccountDO curAccount);
        IEnumerable<ActivityTemplateDTO> GetAvailableActivities(IUnitOfWork uow, Func<ActivityTemplateDO, bool> predicate);

        PlanNodeDO GetNextActivity(PlanNodeDO currentActivity, PlanNodeDO root);
        PlanNodeDO GetNextSibling(PlanNodeDO currentActivity);
        PlanNodeDO GetParent(PlanNodeDO currentActivity);
        PlanNodeDO GetFirstChild(PlanNodeDO currentActivity);
        bool HasChildren(PlanNodeDO currentActivity);

        void Delete(IUnitOfWork uow, PlanNodeDO activity);

        IEnumerable<ActivityTemplateCategoryDTO> GetAvailableActivityGroups();
        IEnumerable<ActivityTemplateCategoryDTO> GetActivityTemplatesGroupedByCategories();

        IEnumerable<ActivityTemplateDTO> GetSolutions(IUnitOfWork uow);
    }
}