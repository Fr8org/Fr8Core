using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Crates;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;


namespace Hub.Interfaces
{
	public interface IRouteNode
	{
		List<RouteNodeDO> GetUpstreamActivities(IUnitOfWork uow, RouteNodeDO curActivityDO);

        List<RouteNodeDO> GetDownstreamActivities(IUnitOfWork uow, RouteNodeDO curActivityDO);

        Task Process(Guid curActivityId, ContainerDO curContainerDO);

        IEnumerable<ActivityTemplateDTO> GetAvailableActivities(IUnitOfWork uow, IFr8AccountDO curAccount);
        IEnumerable<ActivityTemplateDTO> GetAvailableActivities(IUnitOfWork uow, Func<ActivityTemplateDO, bool> predicate);

        RouteNodeDO GetNextActivity(RouteNodeDO currentActivity, RouteNodeDO root);
        RouteNodeDO GetNextSibling(RouteNodeDO currentActivity);
        RouteNodeDO GetFirstChild(RouteNodeDO currentActivity);

        void Delete(IUnitOfWork uow, RouteNodeDO activity);

        IEnumerable<ActivityTemplateCategoryDTO> GetAvailableActivitiyGroups();

        Task<List<Crate<TManifest>>> GetCratesByDirection<TManifest>(Guid activityId, CrateDirection direction);

        Task<List<Crate>> GetCratesByDirection(Guid activityId, CrateDirection direction);
	    
        IEnumerable<ActivityTemplateDTO> GetSolutions(IUnitOfWork uow, IFr8AccountDO curAccount);
    }
}