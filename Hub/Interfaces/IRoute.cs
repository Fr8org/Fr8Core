using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using System.Threading.Tasks;

namespace Hub.Interfaces
{
	public interface IRoute
	{
        IList<RouteDO> GetForUser(IUnitOfWork uow, Fr8AccountDO account, bool isAdmin, int? id = null, int? status = null);
		void CreateOrUpdate(IUnitOfWork uow, RouteDO ptdo, bool withTemplate);
	    RouteDO Create(IUnitOfWork uow, string name);
        void Delete(IUnitOfWork uow, int id);
	    RouteNodeDO GetInitialActivity(IUnitOfWork uow, RouteDO curRoute);

        IList<SubrouteDO> GetSubroutes(RouteDO curRouteDO);
        IList<RouteDO> GetMatchingRoutes(string userId, EventReportCM curEventReport);
        RouteNodeDO GetFirstActivity(int curRouteId);
        Task<string> Activate(RouteDO curRoute);
        Task<string> Deactivate(RouteDO curRoute);
        IEnumerable<ActionDO> GetActions(int id);
	    RouteDO GetRoute(ActionDO action);
	    RouteDTO MapRouteToDto(IUnitOfWork uow, RouteDO curRouteDO);
	  //  ActionListDO GetActionList(IUnitOfWork uow, int id);
        List<RouteDO> MatchEvents(List<RouteDO> curRoutes, EventReportCM curEventReport);

        RouteDO Copy(IUnitOfWork uow, RouteDO curRouteDO, string name);
	}
    }