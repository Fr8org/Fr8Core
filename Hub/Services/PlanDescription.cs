using Data.Entities;
using Data.Interfaces;
using Hub.Interfaces;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Services
{
    public class PlanDescription : IPlanDescription
    {
        public PlanDescriptionDO Save(Guid planId, string curFr8UserId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = uow.PlanRepository.GetById<PlanDO>(planId);

                var planDescription = new PlanDescriptionDO();
                int existingCount = uow.PlanDescriptionsRepository.GetQuery().Where(a => a.Name.Contains(plan.Name) && a.Fr8AccountId == curFr8UserId).Count();

                string name = plan.Name;
                if (existingCount > 0)
                    name += string.Format(" #{0}", ++existingCount);

                planDescription.Name = name;
                planDescription.Fr8AccountId = curFr8UserId;

                //planDescription.PlanNodeDescriptions = new List<PlanNodeDescriptionDO>();
                //foreach (var subplan in plan.SubPlans.Where(a => a.ChildNodes.Count > 0))
                //{
                //    PlanNodeDescriptionDO planNode = new PlanNodeDescriptionDO();

                //    var firstActivity = subplan.ChildNodes[0];
                //  //  firstActivity.

                //    for (int i = 1; i < subplan.ChildNodes.Count; i++)
                //    {

                //    }
                //}


                uow.PlanDescriptionsRepository.Add(planDescription);
                uow.SaveChanges();


                return planDescription;
            }
        }
    }
}
