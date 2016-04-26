using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects.PlanDescription;
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
        public List<PlanDescriptionDTO> GetDescriptions(string userId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                return uow.PlanDescriptionsRepository.GetQuery().Where(a => a.Fr8AccountId == userId).ToList().Select(b => Mapper.Map<PlanDescriptionDTO>(b)).ToList();
            }
        }

        public PlanDescriptionDTO Save(Guid planId, string curFr8UserId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = uow.PlanRepository.GetById<PlanDO>(planId);

                var all_activities = uow.PlanRepository.GetActivityQueryUncached().Where(act => act.RootPlanNodeId == plan.Id);
                var related_templates = uow.ActivityTemplateRepository.GetQuery().Where(a => all_activities.Any(b => b.ActivityTemplateId == a.Id));


                var planDescription = new PlanDescriptionDO();
                int existingCount = uow.PlanDescriptionsRepository.GetQuery().Where(a => a.Name.Contains(plan.Name) && a.Fr8AccountId == curFr8UserId).Count();

                string name = plan.Name;
                if (existingCount > 0)
                    name += string.Format(" #{0}", ++existingCount);

                planDescription.Name = name;
                planDescription.Fr8AccountId = curFr8UserId;

                planDescription.PlanNodeDescriptions = new List<PlanNodeDescriptionDO>();
                foreach (var subplan in plan.SubPlans.Where(a => a.ChildNodes.Count > 0))
                {

                    foreach (var activityNode in subplan.ChildNodes)
                    {
                        var firstActivity = all_activities.Where(a => a.Id == activityNode.Id).FirstOrDefault();
                        var template = related_templates.Where(a => a.Id == firstActivity.ActivityTemplateId).FirstOrDefault();
                        var node = CreatePlanNode(firstActivity, template);
                        planDescription.PlanNodeDescriptions.Add(node);
                    }

                    //add transitions
                    PlanNodeDescriptionDO parentNode = null;
                    foreach (var activityNode in subplan.ChildNodes)
                    {

                        int index = subplan.ChildNodes.IndexOf(activityNode);
                        var plannode = planDescription.PlanNodeDescriptions[index];

                        //if (parentNode != null)
                        //{
                        //    parentNode.Transitions = new List<ActivityTransitionDO>();
                        //    parentNode.Transitions.Add(new ActivityTransitionDO() { Transition = PlanNodeTransitionType.Child, ActivityDescription = plannode.ActivityDescription });
                        //}


                        plannode.ParentNode = parentNode ?? plannode;
                        parentNode = plannode;


                    }
                }


                uow.PlanDescriptionsRepository.Add(planDescription);
                uow.SaveChanges();

                foreach (var subplan in plan.SubPlans.Where(a => a.ChildNodes.Count > 0))
                {
                    //add transitions
                    PlanNodeDescriptionDO parentNode = null;
                    foreach (var activityNode in subplan.ChildNodes)
                    {

                        int index = subplan.ChildNodes.IndexOf(activityNode);
                        var plannode = planDescription.PlanNodeDescriptions[index];

                        if (parentNode != null)
                        {
                            parentNode.Transitions = new List<ActivityTransitionDO>();
                            parentNode.Transitions.Add(new ActivityTransitionDO() { Transition = PlanNodeTransitionType.Child, ActivityDescription = plannode.ActivityDescription });
                        }

                        parentNode = plannode;
                    }
                }

                uow.SaveChanges();

                return Mapper.Map<PlanDescriptionDTO>(planDescription);
            }
        }

        private static PlanNodeDescriptionDO CreatePlanNode(ActivityDO firstActivity, ActivityTemplateDO template)
        {
            PlanNodeDescriptionDO planNode = new PlanNodeDescriptionDO();
            planNode.Name = firstActivity.Label;
            planNode.ActivityDescription = new ActivityDescriptionDO()
            {
                ActivityTemplateId = firstActivity.ActivityTemplateId,
                Name = firstActivity.Label,
                Status = ActivityDescriptionStatus.Primary,
                Version = template.Version,
            };
            return planNode;
        }
    }
}
