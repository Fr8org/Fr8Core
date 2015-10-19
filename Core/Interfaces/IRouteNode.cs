using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Enums;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;

namespace Core.Interfaces
{
	public interface IRouteNode
	{
		List<RouteNodeDO> GetUpstreamActivities(IUnitOfWork uow, RouteNodeDO curActivityDO);

        List<RouteNodeDO> GetDownstreamActivities(IUnitOfWork uow, RouteNodeDO curActivityDO);

        Task Process(int curActivityId, ContainerDO curContainerDO);

        IEnumerable<ActivityTemplateDO> GetAvailableActivities(IUnitOfWork uow, IFr8AccountDO curAccount);

        RouteNodeDO GetNextActivity(RouteNodeDO currentActivity, RouteNodeDO root);

	    void Delete(IUnitOfWork uow, RouteNodeDO activity);

        IEnumerable<ActivityTemplateCategoryDTO> GetAvailableActivitiyGroups(IFr8AccountDO curAccount);

	    Task<List<CrateDTO>> GetCratesByDirection(int activityId, string manifestType, GetCrateDirection direction);
	}
}