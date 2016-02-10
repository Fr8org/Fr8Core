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


        public PlanDO CreatePlan(IUnitOfWork uow, Fr8AccountDO account)
        {
            var generatedRouteName = GenerateFindObjectsRouteName(uow, account);

            var plan = new PlanDO()
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
                ParentRouteNode = plan
            };

            var connectToSqlActivityTemplate = _activityTemplate.GetByName(uow, "ConnectToSql_v1");
            var connectToSqlAction = new ActivityDO()
            {
                Id = Guid.NewGuid(),
                Ordering = 1,
                ActivityTemplateId = connectToSqlActivityTemplate.Id,
                Label = connectToSqlActivityTemplate.Name
            };

            var buildQueryActivityTemplate = _activityTemplate.GetByName(uow, "BuildQuery_v1");
            var buildQueryAction = new ActivityDO()
            {
                Id = Guid.NewGuid(),
                Ordering = 2,
                ActivityTemplateId = buildQueryActivityTemplate.Id,
                Label = buildQueryActivityTemplate.Name
            };

            var executeSqlActivityTemplate = _activityTemplate.GetByName(uow, "ExecuteSql_v1");
            var executeSqlAction = new ActivityDO()
            {
                Id = Guid.NewGuid(),
                Ordering = 3,
                ActivityTemplateId = executeSqlActivityTemplate.Id,
                Label = executeSqlActivityTemplate.Name
            };

            var manageRouteActivityTemplate = _activityTemplate.GetByName(uow, "ManageRoute_v1");
            var manageRouteAction = new ActivityDO()
            {
                Id = Guid.NewGuid(),
                Ordering = 4,
                ActivityTemplateId = manageRouteActivityTemplate.Id,
                Label = manageRouteActivityTemplate.Name
            };

            plan.ChildNodes.Add(subroute);
            subroute.StartingSubroute = true;
            subroute.ChildNodes.Add(connectToSqlAction);
            subroute.ChildNodes.Add(buildQueryAction);
            subroute.ChildNodes.Add(executeSqlAction);
            subroute.ChildNodes.Add(manageRouteAction);

            uow.PlanRepository.Add(plan);

            return plan;
        }

        private string GenerateFindObjectsRouteName(
            IUnitOfWork uow, Fr8AccountDO account)
        {
            var findObjectRoutes = uow.PlanRepository.GetPlanQueryUncached()
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
