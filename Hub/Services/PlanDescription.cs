using AutoMapper;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects.PlanDescription;
using Hub.Interfaces;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hub.Services
{
    public class PlanDescription : IPlanDescription
    {
        private IPlan _plan;

        public PlanDescription()
        {
            _plan = ObjectFactory.GetInstance<IPlan>();
        }

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
                Dictionary<Guid, PlanNodeDescriptionDO> nodes_dictionary = new Dictionary<Guid, PlanNodeDescriptionDO>();

                var planDescription = new PlanDescriptionDO();
                var descriptions = uow.PlanDescriptionsRepository.GetQuery().Where(a => a.Name.Contains(plan.Name) && a.Fr8AccountId == curFr8UserId).ToList();
                string name = CalculateName(curFr8UserId, descriptions, plan);

                planDescription.Name = name;
                planDescription.Description = plan.Description;
                planDescription.Fr8AccountId = curFr8UserId;
                planDescription.PlanNodeDescriptions = new List<PlanNodeDescriptionDO>();
                uow.PlanDescriptionsRepository.Add(planDescription);

                foreach (var subplan in plan.SubPlans.Where(a => a.ChildNodes.Count > 0))
                {
                    BuildNodes(subplan, planDescription, all_activities, related_templates, plan.StartingSubPlanId == subplan.Id);
                }

                uow.SaveChanges();

                return Mapper.Map<PlanDescriptionDTO>(planDescription);
            }
        }


        public string BuildAPlan(int planDescriptionId, string userId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var planDescription = uow.PlanDescriptionsRepository.GetQuery()
                    .Include(t => t.PlanNodeDescriptions.Select(x => x.ActivityDescription))
                    .Include(t => t.PlanNodeDescriptions.Select(x => x.Transitions))
                    .Where(a => a.Id == planDescriptionId).FirstOrDefault();

                var planDO = new PlanDO() { Fr8AccountId = userId, Name = planDescription.Name, PlanState = 1, ChildNodes = new List<PlanNodeDO>(), Description = planDescription.Description };

                var first_activities = planDescription.PlanNodeDescriptions.Where(a => a.ParentNode == null);
                foreach (var startingActivity in first_activities)
                {
                    var subplan = new SubPlanDO() { Name = startingActivity.SubPlanName };
                    if (planDescription.StartingPlanNodeDescription.Id == startingActivity.Id)
                        planDO.StartingSubPlan = subplan;
                    else
                        planDO.ChildNodes.Add(subplan);
                    BuildAPlan(userId, 1, subplan, planDescription, startingActivity, planDO);
                }






                uow.PlanRepository.Add(planDO);

                uow.SaveChanges();

                return planDO.Id.ToString();
            }
        }

        private void BuildAPlan(string userId, int ordering, PlanNodeDO planNodeDO, PlanDescriptionDO planDescription, PlanNodeDescriptionDO parentNode, PlanDO planDO)
        {
            ActivityDO parentNodeActivity = CreateActivityDO(userId, ordering, planNodeDO, parentNode, planDO);
            planNodeDO.ChildNodes.Add(parentNodeActivity);

            var child_nodes = planDescription.PlanNodeDescriptions.Where(a => a.ParentNode == parentNode);
            foreach (var child in child_nodes)
            {
                var transition = parentNode.Transitions.Where(a => a.ActivityDescriptionId == child.ActivityDescription.Id).FirstOrDefault().Transition;
                if (transition == PlanNodeTransitionType.Downstream)
                {
                    BuildAPlan(userId, ++ordering, planNodeDO, planDescription, child, planDO);
                }
                if (transition == PlanNodeTransitionType.Child)
                {
                    BuildAPlan(userId, ordering, parentNodeActivity, planDescription, child, planDO);
                }
            }
        }

        private static ActivityDO CreateActivityDO(string userId, int ordering, PlanNodeDO planNodeDO, PlanNodeDescriptionDO parentNode, PlanDO planDO)
        {
            var activityDO = new ActivityDO();
            activityDO.CrateStorage = parentNode.ActivityDescription.CrateStorage;
            activityDO.ActivityTemplateId = parentNode.ActivityDescription.ActivityTemplateId;
            activityDO.Fr8AccountId = userId;
            activityDO.Label = parentNode.Name;
            activityDO.Ordering = ordering;
            activityDO.ParentPlanNode = planNodeDO;
            activityDO.RootPlanNode = planDO;
            return activityDO;
        }

        private string CalculateName(string curFr8UserId, List<PlanDescriptionDO> descriptions, PlanDO plan)
        {
            var existingDescriptionWithLargestNumber = descriptions.OrderBy(a => a.Name.Length).ThenBy(b => b.Name).LastOrDefault();
            string name = plan.Name;
            if (existingDescriptionWithLargestNumber != null)
            {
                string largestNumber = "0";
                if (existingDescriptionWithLargestNumber.Name.Length != plan.Name.Length)
                {
                    largestNumber = existingDescriptionWithLargestNumber.Name.Substring((plan.Name + " #").Length);
                }
                int number = Convert.ToInt32(largestNumber) + 1;
                name += string.Format(" #{0}", number);
            }
            return name;
        }


        private Dictionary<Guid, PlanNodeDescriptionDO> BuildNodes(PlanNodeDO nodeDO, PlanDescriptionDO planDescription, IQueryable<ActivityDO> all_activities, IQueryable<ActivityTemplateDO> related_templates, bool startingSubplan)
        {
            var result = new Dictionary<Guid, PlanNodeDescriptionDO>();
            foreach (var activityNode in nodeDO.ChildNodes)
            {
                var firstActivity = all_activities.Where(a => a.Id == activityNode.Id).FirstOrDefault();
                var template = related_templates.Where(a => a.Id == firstActivity.ActivityTemplateId).FirstOrDefault();
                var node = CreatePlanNode(firstActivity, template);

                if (nodeDO is SubPlanDO)
                    node.SubPlanName = (nodeDO as SubPlanDO).Name;

                if (startingSubplan)
                {
                    planDescription.StartingPlanNodeDescription = node;
                    startingSubplan = false;
                }
                else
                    planDescription.PlanNodeDescriptions.Add(node);

                result.Add(firstActivity.Id, node);

                var child_nodes = BuildNodes(activityNode, planDescription, all_activities, related_templates, startingSubplan);

                //handle childs
                foreach (var child in child_nodes)
                {
                    node.Transitions.Add(new ActivityTransitionDO() { ActivityDescription = child.Value.ActivityDescription, Transition = PlanNodeTransitionType.Child });
                    child.Value.ParentNode = node;
                    child.Value.SubPlanName = node.SubPlanName;
                }
            }

            foreach (var activityNode in nodeDO.ChildNodes)
            {
                var current_node = result[activityNode.Id];
                //handle upstream/downstream
                var upstream_activity = nodeDO.ChildNodes.Where(a => a.Id != activityNode.Id && a.Ordering == (activityNode.Ordering - 1)).FirstOrDefault();
                if (upstream_activity != null)
                {
                    var upstream_node = result[upstream_activity.Id];
                    upstream_node.Transitions.Add(new ActivityTransitionDO() { ActivityDescription = current_node.ActivityDescription, Transition = PlanNodeTransitionType.Downstream });
                    current_node.ParentNode = upstream_node;
                }
            }

            return result;
        }

        private PlanNodeDescriptionDO CreatePlanNode(ActivityDO firstActivity, ActivityTemplateDO template)
        {
            PlanNodeDescriptionDO planNode = new PlanNodeDescriptionDO();
            planNode.Name = firstActivity.Label;
            planNode.ActivityDescription = new ActivityDescriptionDO()
            {
                ActivityTemplateId = firstActivity.ActivityTemplateId,
                Name = firstActivity.Label,
                Status = ActivityDescriptionStatus.Primary,
                Version = template.Version,
                CrateStorage = firstActivity.CrateStorage
            };
            planNode.Transitions = new List<ActivityTransitionDO>();
            return planNode;
        }
    }
}
