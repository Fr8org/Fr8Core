using System;
using System.Collections.Generic;
using System.Linq;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Fr8.Infrastructure.Data.Control;
using Fr8.Infrastructure.Data.DataTransferObjects.PlanTemplates;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Hub.Interfaces;
using Hub.Managers;

namespace Hub.Services
{
    public class PlanTemplates : IPlanTemplates
    {
        private IPlan _plan;
        private ICrateManager _crateManager;

        public PlanTemplates()
        {
            _plan = ObjectFactory.GetInstance<IPlan>();
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }

        public PlanTemplateDTO GetPlanTemplate(Guid planId, string curFr8UserId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var plan = uow.PlanRepository.GetById<PlanDO>(planId);
            
                var allActivities = plan.GetDescendants().OfType<ActivityDO>().ToList();
                var allActivitiesTemplates = string.Join(",", allActivities.Select(a => a.ActivityTemplateId));
                var relatedTemplates = uow.ActivityTemplateRepository.GetQuery().Where(a => allActivitiesTemplates.Contains(a.Id.ToString())).ToList();
                var nodesDictionary = new Dictionary<Guid, PlanNodeDescriptionDTO>();

                var planTemplate = new PlanTemplateDTO() { Id = Guid.NewGuid() };

                var name = plan.Name;
                planTemplate.Name = name;
                planTemplate.Description = plan.Description;
                planTemplate.PlanNodeDescriptions = new List<PlanNodeDescriptionDTO>();
            
                var allNodesDictionary = new Dictionary<Guid, PlanNodeDescriptionDTO>();
                foreach (var subplan in plan.SubPlans.Where(a => a.ChildNodes.Count > 0))
                {
                    BuildPlanTemplateNodes(
                        subplan,
                        planTemplate,
                        allActivities,
                        relatedTemplates,
                        plan.StartingSubPlanId == subplan.Id,
                        ref allNodesDictionary
                    );
                }
            
                BuildJumpTransitions(
                    allNodesDictionary,
                    allActivities,
                    relatedTemplates,
                    planTemplate,
                    plan
                );
            
                return planTemplate;
            }
        }

        public PlanDO LoadPlan(PlanTemplateDTO planTemplate, string fr8UserId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {       
                var planDO = new PlanDO()
                {
                    Id = Guid.NewGuid(),
                    Fr8AccountId = fr8UserId,
                    Name = planTemplate.Name,// + " — from PlanDirectory",
                    PlanState = 1,
                    ChildNodes = new List<PlanNodeDO>(),
                    Description = planTemplate.Description
                };
        
                var allNodesDictionary = new Dictionary<ActivityDO, PlanNodeDescriptionDTO>();
                var firstActivities = planTemplate.PlanNodeDescriptions.Where(a => a.ParentNodeId == null);
                foreach (var startingActivity in firstActivities)
                {
                    var subplan = new SubplanDO() { Name = startingActivity.SubPlanName };
                    if (planTemplate.StartingPlanNodeDescriptionId == startingActivity.Id)
                    {
                        planDO.StartingSubplan = subplan;
                    }
                    else
                    {
                        planDO.ChildNodes.Add(subplan);
                    }

                    BuildPlanNodes(
                        fr8UserId,
                        1,
                        subplan,
                        planTemplate,
                        startingActivity,
                        planDO,
                        ref allNodesDictionary
                    );
                }
        
                uow.PlanRepository.Add(planDO);
        
                UpdateJumpTransitions(planDO, planTemplate, allNodesDictionary);
        
                uow.SaveChanges();
        
                return planDO;
            }
        }

        private Dictionary<Guid, PlanNodeDescriptionDTO> BuildPlanTemplateNodes(
            PlanNodeDO nodeDO, PlanTemplateDTO planTemplate, List<ActivityDO> allActivities,
            List<ActivityTemplateDO> relatedTemplates, bool startingSubplan,
            ref Dictionary<Guid, PlanNodeDescriptionDTO> allNodesDictionary)
        {
            var result = new Dictionary<Guid, PlanNodeDescriptionDTO>();
            foreach (var activityNode in nodeDO.ChildNodes.OrderBy(a => a.Ordering))
            {
                var firstActivity = allActivities.Where(a => a.Id == activityNode.Id).FirstOrDefault();
                var template = relatedTemplates.Where(a => a.Id == firstActivity.ActivityTemplateId).FirstOrDefault();
                var node = CreatePlanNodeDescription(firstActivity, template);
        
                if (nodeDO is SubplanDO)
                {
                    node.SubPlanName = (nodeDO as SubplanDO).Name;
                    node.SubPlanOriginalId = nodeDO.Id.ToString();
                }

                if (startingSubplan)
                {
                    planTemplate.StartingPlanNodeDescriptionId = node.Id;
                    startingSubplan = false;
                }
                
                planTemplate.PlanNodeDescriptions.Add(node);
        
                result.Add(firstActivity.Id, node);
                if (!allNodesDictionary.ContainsKey(firstActivity.Id))
                {
                    allNodesDictionary.Add(firstActivity.Id, node);
                }
        
        
                //Handle children transitions
                var childNodes = BuildPlanTemplateNodes(
                    activityNode,
                    planTemplate,
                    allActivities,
                    relatedTemplates,
                    startingSubplan,
                    ref allNodesDictionary
                );

                if (childNodes.Count > 0)
                {
                    var childActivities = new List<ActivityDO>();
                    foreach (var activity in allActivities)
                    {
                        if (childNodes.ContainsKey(activity.Id))
                        {
                            childActivities.Add(activity);
                        }
                    }

                    var singleChild = childActivities.OrderBy(a => a.Ordering).First().Id;
                    var child = childNodes[singleChild];
                    node.Transitions.Add(new NodeTransitionDTO()
                    {
                        ActivityDescriptionId = child.ActivityDescription.Id,
                        Transition = PlanNodeTransitionType.Child.ToString()
                    });
                    child.ParentNodeId = node.Id;
                    child.SubPlanName = node.SubPlanName;
                }
            }
        
            //handle upstream/downstream transitions
            foreach (var activityNode in nodeDO.ChildNodes)
            {
                var currentNode = result[activityNode.Id];
                var upstreamActivity = nodeDO.ChildNodes
                    .Where(a => a.Id != activityNode.Id && a.Ordering == (activityNode.Ordering - 1))
                    .FirstOrDefault();
                if (upstreamActivity != null)
                {
                    var upstreamNode = result[upstreamActivity.Id];
                    upstreamNode.Transitions.Add(new NodeTransitionDTO()
                    {
                        ActivityDescriptionId = currentNode.ActivityDescription.Id,
                        Transition = PlanNodeTransitionType.Downstream.ToString()
                    });

                    currentNode.ParentNodeId = upstreamNode.Id;
                }
            }
        
            return result;
        }

        private PlanNodeDescriptionDTO CreatePlanNodeDescription(
            ActivityDO firstActivity, ActivityTemplateDO template)
        {
            PlanNodeDescriptionDTO planNode = new PlanNodeDescriptionDTO()
            {
                Id = Guid.NewGuid()
            };

            planNode.Name = firstActivity.Label;
            planNode.ActivityDescription = new ActivityDescriptionDTO()
            {
                Id = Guid.NewGuid(),
                ActivityTemplateId = firstActivity.ActivityTemplateId,
                OriginalId = firstActivity.Id.ToString(),
                Name = firstActivity.Label ?? firstActivity.Name,
                Version = template.Version,
                CrateStorage = firstActivity.CrateStorage
            };
            planNode.Transitions = new List<NodeTransitionDTO>();
            return planNode;
        }

        private void BuildJumpTransitions(
            Dictionary<Guid, PlanNodeDescriptionDTO> allNodesDictionary,
            List<ActivityDO> allActivities,
            List<ActivityTemplateDO> relatedTemplates,
            PlanTemplateDTO planTemplate, 
            PlanDO plan)
        {
            var makeADecisionTemplate = FindMakeADecisionTemplate(relatedTemplates.ToList());
            if (makeADecisionTemplate == null) return;
        
            var makeADecisionActivities = allActivities.Where(a => a.ActivityTemplateId == makeADecisionTemplate.Id).ToList();
        
            foreach (var tabActivity in makeADecisionActivities)
            {
                var makeADecisionNode = allNodesDictionary[tabActivity.Id];
                using (var crateStorage = _crateManager.GetUpdatableStorage(tabActivity))
                {
                    var controlsCrate = crateStorage.Where(a => a.ManifestType.Id == (int)Fr8.Infrastructure.Data.Constants.MT.StandardConfigurationControls).FirstOrDefault().Get<StandardConfigurationControlsCM>();
                    var transitionsPanel = (ContainerTransition)controlsCrate.Controls.Where(a => a.Type == ControlTypes.ContainerTransition).FirstOrDefault();
        
                    foreach (var transition in transitionsPanel.Transitions.Where(a => a.TargetNodeId.HasValue))
                    {
                        switch (transition.Transition)
                        {
                            case ContainerTransitions.JumpToActivity:
                                var targetNode = allNodesDictionary[transition.TargetNodeId.Value];
                                makeADecisionNode.Transitions.Add(
                                    new NodeTransitionDTO()
                                    {
                                        ActivityDescriptionId = targetNode.ActivityDescription.Id,
                                        Transition = PlanNodeTransitionType.Jump.ToString()
                                    }
                                );
                                break;
        
                            case ContainerTransitions.JumpToSubplan:
                                var subplan = plan.SubPlans.Where(a => a.Id == transition.TargetNodeId.Value).FirstOrDefault();
                                var targetStartingNodeDO = subplan.ChildNodes.Where(a => a.Ordering == 1).FirstOrDefault();
        
                                if (targetStartingNodeDO != null) //false if a jump is targeting an emty subplan, so we won't create transitions
                                {
                                    var targetStartingNode = allNodesDictionary[targetStartingNodeDO.Id];
                                    makeADecisionNode.Transitions.Add(
                                        new NodeTransitionDTO()
                                        {
                                            ActivityDescriptionId = targetStartingNode.ActivityDescription.Id,
                                            Transition = PlanNodeTransitionType.Jump.ToString()
                                        }
                                    );
                                }
                                break;
        
                            case ContainerTransitions.LaunchAdditionalPlan:
                                // for now let's jump to an existing a plan, but that will lead to "unsharable" plan description,
                                // so in future we might want to evaluate if a user wants a target plan to be saved as description as well
                                makeADecisionNode.Transitions.Add(
                                    new NodeTransitionDTO()
                                    {
                                        PlanId = transition.TargetNodeId,
                                        Transition = PlanNodeTransitionType.Jump.ToString()
                                    }
                                );
                                break;
                        }
                    }
                }
        
            }
        }

        private ActivityTemplateDO FindMakeADecisionTemplate(List<ActivityTemplateDO> templates)
        {
            return templates.Where(a => a.Name == "Make_A_Decision").FirstOrDefault();
        }

        private void BuildPlanNodes(string userId, int ordering, PlanNodeDO planNodeDO,
            PlanTemplateDTO planTemplate, PlanNodeDescriptionDTO parentNode,
            PlanDO planDO, ref Dictionary<ActivityDO, PlanNodeDescriptionDTO> allNodesDictionary)
        {
            var parentNodeActivity = CreateActivityDO(userId, ordering, planNodeDO, parentNode, planDO);
            planNodeDO.ChildNodes.Add(parentNodeActivity);
            allNodesDictionary.Add(parentNodeActivity, parentNode);
        
            var childNodes = planTemplate.PlanNodeDescriptions.Where(a => a.ParentNodeId == parentNode.Id);
            foreach (var child in childNodes)
            {
                var transitionString = parentNode.Transitions.Where(a => a.ActivityDescriptionId == child.ActivityDescription.Id).FirstOrDefault().Transition;
                var transition = (PlanNodeTransitionType)Enum.Parse(typeof(PlanNodeTransitionType), transitionString);
                if (transition == PlanNodeTransitionType.Downstream)
                {
                    BuildPlanNodes(userId, ++ordering, planNodeDO, planTemplate, child, planDO, ref allNodesDictionary);
                }
                if (transition == PlanNodeTransitionType.Child)
                {
                    BuildPlanNodes(userId, ordering, parentNodeActivity, planTemplate, child, planDO, ref allNodesDictionary);
                }
            }
        }
        
        private ActivityDO CreateActivityDO(string userId, int ordering,
            PlanNodeDO planNodeDO, PlanNodeDescriptionDTO parentNode, PlanDO planDO)
        {
            var activityDO = new ActivityDO();
            activityDO.CrateStorage = parentNode.ActivityDescription.CrateStorage;
            activityDO.ActivityTemplateId = parentNode.ActivityDescription.ActivityTemplateId;
            activityDO.Fr8AccountId = userId;
            activityDO.Label = parentNode.ActivityDescription.Name;
            activityDO.Ordering = ordering;
            activityDO.ParentPlanNode = planNodeDO;
            activityDO.RootPlanNode = planDO;

            return activityDO;
        }

        private void UpdateJumpTransitions(PlanDO planDO, PlanTemplateDTO planDescription,
            Dictionary<ActivityDO, PlanNodeDescriptionDTO> allNodesDictionary)
        {
            //get TAB activities
            var makeADecisionActivities = new List<ActivityDO>();
            var allActivitiesTemplatesIds = string.Join(",", allNodesDictionary.Select(a => a.Value).Select(b => b.ActivityDescription).Select(c => c.ActivityTemplateId).ToArray());

            var usedTemplates = new List<ActivityTemplateDO>();
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                usedTemplates = uow.ActivityTemplateRepository.GetQuery().Where(a => allActivitiesTemplatesIds.Contains(a.Id.ToString())).ToList();
                var makeADecisionTemplate = FindMakeADecisionTemplate(usedTemplates);
                if (makeADecisionTemplate == null) return;
                var all_used_activities = GetAllPlanActivities(planDO);
                makeADecisionActivities = all_used_activities.Where(a => a.ActivityTemplateId == makeADecisionTemplate.Id).ToList();
                if (makeADecisionActivities.Count == 0) return;
            }
        
            //process
            foreach (var makeADecisionActivity in makeADecisionActivities)
            {
                using (var crateStorage = _crateManager.GetUpdatableStorage(makeADecisionActivity))
                {
                    var controlsCrate = crateStorage.Where(a => a.ManifestType.Id == (int)Fr8.Infrastructure.Data.Constants.MT.StandardConfigurationControls).FirstOrDefault().Get<StandardConfigurationControlsCM>();
                    var transitionsPanel = (ContainerTransition)controlsCrate.Controls.Where(a => a.Type == ControlTypes.ContainerTransition).FirstOrDefault();
        
                    foreach (var transition in transitionsPanel.Transitions)
                    {
                        if (transition.TargetNodeId != null)
                        {
                            switch (transition.Transition)
                            {
                                case ContainerTransitions.JumpToActivity:
                                    var targetActivityNode = planDescription.PlanNodeDescriptions.Where(a => a.ActivityDescription.OriginalId == transition.TargetNodeId.ToString()).FirstOrDefault();
                                    var targetActivity = allNodesDictionary.Where(a => a.Value == targetActivityNode).FirstOrDefault().Key;
                                    transition.TargetNodeId = targetActivity.Id;
                                    break;
                                case ContainerTransitions.JumpToSubplan:
                                    var targetStartingActivityNode = planDescription.PlanNodeDescriptions.Where(a => a.SubPlanOriginalId == transition.TargetNodeId.ToString()).FirstOrDefault();
                                    var startingActivity = allNodesDictionary.Where(a => a.Value == targetStartingActivityNode).FirstOrDefault().Key;
                                    transition.TargetNodeId = startingActivity.ParentPlanNode.Id;
                                    break;
                                case ContainerTransitions.LaunchAdditionalPlan:
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private List<ActivityDO> GetAllPlanActivities(PlanNodeDO planNode)
        {
            var result = new List<ActivityDO>();
            foreach (var child in planNode.ChildNodes)
            {
                if (child is ActivityDO)
                {
                    result.Add((ActivityDO)child);
                }

                var childrenNodes = GetAllPlanActivities(child);
                result.AddRange(childrenNodes);
            }
        
            return result;
        }
    }
}
