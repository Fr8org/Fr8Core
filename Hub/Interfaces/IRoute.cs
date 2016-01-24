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
    public interface IRoute
    {
        IList<RouteDO> GetForUser(IUnitOfWork uow, Fr8AccountDO account, bool isAdmin, Guid? id = null, int? status = null);
        IList<RouteDO> GetByName(IUnitOfWork uow, Fr8AccountDO account, string name);
        void CreateOrUpdate(IUnitOfWork uow, RouteDO ptdo, bool withTemplate);
        RouteDO Create(IUnitOfWork uow, string name);
        void Delete(IUnitOfWork uow, Guid id);
        RouteNodeDO GetInitialActivity(IUnitOfWork uow, RouteDO curRoute);

        IList<SubrouteDO> GetSubroutes(RouteDO curRouteDO);
        IList<RouteDO> GetMatchingRoutes(string userId, EventReportCM curEventReport);
        RouteNodeDO GetFirstActivity(Guid curRouteId);
        Task<ActivateActionsDTO> Activate(Guid routeId, bool routeBuilderActivate);
        Task<string> Deactivate(Guid curRouteId);
        IEnumerable<ActivityDO> GetActivities(int id);
        RouteDO GetRoute(ActivityDO activity);
        //  ActionListDO GetActionList(IUnitOfWork uow, int id);
        List<RouteDO> MatchEvents(List<RouteDO> curRoutes, EventReportCM curEventReport);

        RouteDO Copy(IUnitOfWork uow, RouteDO curRouteDO, string name);


        ContainerDO Create(IUnitOfWork uow, Guid routeId, Crate curEvent);
        Task<ContainerDO> Run(RouteDO curRoute, Crate curEvent);
        Task<ContainerDO> Continue(Guid containerId);
    }
}    
