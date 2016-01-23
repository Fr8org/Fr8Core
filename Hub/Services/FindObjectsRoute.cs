using System;
using System.Linq;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Hub.Interfaces;

namespace Hub.Services
{
    public class FindObjectsRoute : IFindObjectsRoute
    {
        private readonly IActivityTemplate _activityTemplate =
            ObjectFactory.GetInstance<IActivityTemplate>();


        public RouteDO CreateRoute(IUnitOfWork uow, Fr8AccountDO account)
        {
            var generatedRouteName = GenerateFindObjectsRouteName(uow, account);

            var route = new RouteDO()
            {
                Id = Guid.NewGuid(),
                Name = generatedRouteName,
                Description = "Find Objects ",
                Fr8Account = account,
                RouteState = RouteState.Inactive,
                Tag = "Query"
            };

            var subroute = new SubrouteDO()
            {
                Id = Guid.NewGuid(),
                RootRouteNode = route,
                ParentRouteNode = route
            };

            var connectToSqlActivityTemplate = _activityTemplate.GetByName(uow, "ConnectToSql_v1");
            var connectToSqlAction = new ActionDO()
            {
                Id = Guid.NewGuid(),
                ParentRouteNode = subroute,
                Ordering = 1,
                ActivityTemplateId = connectToSqlActivityTemplate.Id,
                Name = connectToSqlActivityTemplate.Name,
                Label = connectToSqlActivityTemplate.Name
            };

            var buildQueryActivityTemplate = _activityTemplate.GetByName(uow, "BuildQuery_v1");
            var buildQueryAction = new ActionDO()
            {
                Id = Guid.NewGuid(),
                ParentRouteNode = subroute,
                Ordering = 2,
                ActivityTemplateId = buildQueryActivityTemplate.Id,
                Name = buildQueryActivityTemplate.Name,
                Label = buildQueryActivityTemplate.Name
            };

            var executeSqlActivityTemplate = _activityTemplate.GetByName(uow, "ExecuteSql_v1");
            var executeSqlAction = new ActionDO()
            {
                Id = Guid.NewGuid(),
                ParentRouteNode = subroute,
                Ordering = 3,
                ActivityTemplateId = executeSqlActivityTemplate.Id,
                Name = executeSqlActivityTemplate.Name,
                Label = executeSqlActivityTemplate.Name
            };

            var manageRouteActivityTemplate = _activityTemplate.GetByName(uow, "ManageRoute_v1");
            var manageRouteAction = new ActionDO()
            {
                Id = Guid.NewGuid(),
                ParentRouteNode = subroute,
                Ordering = 4,
                ActivityTemplateId = manageRouteActivityTemplate.Id,
                Name = manageRouteActivityTemplate.Name,
                Label = manageRouteActivityTemplate.Name
            };

            route.ChildNodes.Add(subroute);
            subroute.StartingSubroute = true;
            subroute.ChildNodes.Add(connectToSqlAction);
            subroute.ChildNodes.Add(buildQueryAction);
            subroute.ChildNodes.Add(executeSqlAction);
            subroute.ChildNodes.Add(manageRouteAction);

            uow.RouteNodeRepository.Add(route);
            uow.RouteNodeRepository.Add(subroute);
            uow.RouteNodeRepository.Add(connectToSqlAction);
            uow.RouteNodeRepository.Add(buildQueryAction);
            uow.RouteNodeRepository.Add(executeSqlAction);
            uow.RouteNodeRepository.Add(manageRouteAction);

            return route;
        }

        private string GenerateFindObjectsRouteName(
            IUnitOfWork uow, Fr8AccountDO account)
        {
            var findObjectRoutes = uow.RouteRepository
                .GetQuery()
                .Where(x => x.Fr8Account.Id == account.Id)
                .Where(x => x.Name.StartsWith("FindObjects #"))
                .ToList();

            var maxNumber = 0;

            foreach (var findObjectRoute in findObjectRoutes)
            {
                var number = 0;

                for (var i = "FindObjects #".Length; i < findObjectRoute.Name.Length; ++i)
                {
                    var c = findObjectRoute.Name[i];
                    if (c < '0' && c > '9') { break; }

                    number = number * 10 + (c - '0');
                }

                if (number > maxNumber) { maxNumber = number; }
            }

            return "FindObjects #" + (maxNumber + 1).ToString();
        }
    }
}
