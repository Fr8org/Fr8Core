using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.ManifestSchemas;

namespace Core.Interfaces
{
	public interface IRoute
	{
		IList<RouteDO> GetForUser(IUnitOfWork uow, DockyardAccountDO account, int? id = null,int?status=null);
		void CreateOrUpdate(IUnitOfWork uow, RouteDO ptdo, bool withTemplate);
		void Delete(IUnitOfWork uow, int id);
	    ActivityDO GetInitialActivity(IUnitOfWork uow, RouteDO curRoute);

        IList<SubrouteDO> GetSubroutes(RouteDO curRouteDO);
        IList<RouteDO> GetMatchingRoutes(string userId, EventReportCM curEventReport);
        ActivityDO GetFirstActivity(int curRouteId);
        string Activate(RouteDO curRoute);
        string Deactivate(RouteDO curRoute);
        IEnumerable<ActionDO> GetActions(int id);
	    RouteDO GetRoute(ActionDO action);
	  //  ActionListDO GetActionList(IUnitOfWork uow, int id);
        List<RouteDO> MatchEvents(List<RouteDO> curRoutes, EventReportCM curEventReport);
	}
}