using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Hub.Enums;

namespace Hub.Interfaces
{
	public interface IRouteNode
	{
		List<RouteNodeDO> GetUpstreamActivities(IUnitOfWork uow, RouteNodeDO curActivityDO);

        List<RouteNodeDO> GetDownstreamActivities(IUnitOfWork uow, RouteNodeDO curActivityDO);

        Task Process(int curActivityId, ContainerDO curContainerDO);

        IEnumerable<ActivityTemplateDTO> GetAvailableActivities(IUnitOfWork uow, IFr8AccountDO curAccount);
        IEnumerable<ActivityTemplateDTO> GetAvailableActivities(IUnitOfWork uow, Func<ActivityTemplateDO, bool> predicate);

        RouteNodeDO GetNextActivity(RouteNodeDO currentActivity, RouteNodeDO root);

	    void Delete(IUnitOfWork uow, RouteNodeDO activity);

        IEnumerable<ActivityTemplateCategoryDTO> GetAvailableActivitiyGroups();

	    Task<List<CrateDTO>> GetCratesByDirection(int activityId, string manifestType, GetCrateDirection direction);

        IEnumerable<ActivityTemplateDTO> GetSolutions(IUnitOfWork uow, IFr8AccountDO curAccount);
    }
}