using System;
using System.Linq;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Hub.Interfaces;

namespace Hub.Services
{
    public class FindObjectsPlan : IFindObjectsPlan
    {
        private readonly IActivityTemplate _activityTemplate =
            ObjectFactory.GetInstance<IActivityTemplate>();


        public PlanDO CreatePlan(IUnitOfWork uow, Fr8AccountDO account)
        {
            var generatedPlanName = GenerateFindObjectsPlanName(uow, account);

            var plan = new PlanDO()
            {
                Id = Guid.NewGuid(),
                Name = generatedPlanName,
                Description = "Find Objects ",
                Fr8Account = account,
                PlanState = PlanState.Inactive,
                Tag = "Query"
            };

            var subPlan = new SubPlanDO()
            {
                Id = Guid.NewGuid(),
                ParentPlanNode = plan
            };

            var connectToSqlActivityTemplate = _activityTemplate.GetByName(uow, "ConnectToSql_v1");
            var connectToSqlAction = new ActivityDO()
            {
                Id = Guid.NewGuid(),
                Ordering = 1,
                ActivityTemplateId = connectToSqlActivityTemplate.Id,
                Name = connectToSqlActivityTemplate.Name
            };

            var buildQueryActivityTemplate = _activityTemplate.GetByName(uow, "BuildQuery_v1");
            var buildQueryAction = new ActivityDO()
            {
                Id = Guid.NewGuid(),
                Ordering = 2,
                ActivityTemplateId = buildQueryActivityTemplate.Id,
                Name = buildQueryActivityTemplate.Name
            };

            var executeSqlActivityTemplate = _activityTemplate.GetByName(uow, "ExecuteSql_v1");
            var executeSqlAction = new ActivityDO()
            {
                Id = Guid.NewGuid(),
                Ordering = 3,
                ActivityTemplateId = executeSqlActivityTemplate.Id,
                Name = executeSqlActivityTemplate.Name
            };

            var managePlanActivityTemplate = _activityTemplate.GetByName(uow, "ManagePlan_v1");
            var managePlanAction = new ActivityDO()
            {
                Id = Guid.NewGuid(),
                Ordering = 4,
                ActivityTemplateId = managePlanActivityTemplate.Id,
                Name = managePlanActivityTemplate.Name
            };

            plan.ChildNodes.Add(subPlan);
            subPlan.StartingSubPlan = true;
            subPlan.ChildNodes.Add(connectToSqlAction);
            subPlan.ChildNodes.Add(buildQueryAction);
            subPlan.ChildNodes.Add(executeSqlAction);
            subPlan.ChildNodes.Add(managePlanAction);

            uow.PlanRepository.Add(plan);

            return plan;
        }

        private string GenerateFindObjectsPlanName(
            IUnitOfWork uow, Fr8AccountDO account)
        {
            var findObjectPlans = uow.PlanRepository.GetPlanQueryUncached()
                .Where(x => x.Fr8Account.Id == account.Id)
                .Where(x => x.Name.StartsWith("FindObjects #"))
                .ToList();

            var maxNumber = 0;

            foreach (var findObjectPlan in findObjectPlans)
            {
                var number = 0;

                for (var i = "FindObjects #".Length; i < findObjectPlan.Name.Length; ++i)
                {
                    var c = findObjectPlan.Name[i];
                    if (c < '0' && c > '9') { break; }

                    number = number * 10 + (c - '0');
                }

                if (number > maxNumber) { maxNumber = number; }
            }

            return "FindObjects #" + (maxNumber + 1).ToString();
        }
    }
}
