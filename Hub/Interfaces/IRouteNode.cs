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
	public interface IRouteNode
	{
		List<RouteNodeDO> GetUpstreamActivities(IUnitOfWork uow, RouteNodeDO curActivityDO);

        List<RouteNodeDO> GetDownstreamActivities(IUnitOfWork uow, RouteNodeDO curActivityDO);

        StandardDesignTimeFieldsCM GetDesignTimeFieldsByDirection(Guid activityId, CrateDirection direction, AvailabilityType availability);

        Task Process(Guid curActivityId, ActionState curActionState, ContainerDO curContainerDO);

        IEnumerable<ActivityTemplateDTO> GetAvailableActivities(IUnitOfWork uow, IFr8AccountDO curAccount);
        IEnumerable<ActivityTemplateDTO> GetAvailableActivities(IUnitOfWork uow, Func<ActivityTemplateDO, bool> predicate);

        RouteNodeDO GetNextActivity(RouteNodeDO currentActivity, RouteNodeDO root);
        RouteNodeDO GetNextSibling(RouteNodeDO currentActivity);
        RouteNodeDO GetParent(RouteNodeDO currentActivity);
        RouteNodeDO GetFirstChild(RouteNodeDO currentActivity);
	    bool HasChildren(RouteNodeDO currentActivity);

        void Delete(IUnitOfWork uow, RouteNodeDO activity);

        IEnumerable<ActivityTemplateCategoryDTO> GetAvailableActivitiyGroups();
	    
        IEnumerable<ActivityTemplateDTO> GetSolutions(IUnitOfWork uow, IFr8AccountDO curAccount);
    }
}