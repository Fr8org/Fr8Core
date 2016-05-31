using System;
using System.Collections.Generic;
using System.Linq;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects.PlanTemplates;
using Data.Repositories.Encryption;
using Fr8Data.Control;
using Fr8Data.DataTransferObjects.PlanTemplates;
using Fr8Data.Managers;
using Fr8Data.Manifests;
using Hub.Interfaces;
using Hub.Managers;

namespace Hub.Services
{
    public class PlanTemplates : IPlanTemplates
    {
        private IPlan _plan;
        private ICrateManager _crateManager;
        private IEncryptionService _encryption;

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
                    Name = planTemplate.Name + " — from PlanDirectory",
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
                    var controlsCrate = crateStorage.Where(a => a.ManifestType.Id == (int)Fr8Data.Constants.MT.StandardConfigurationControls).FirstOrDefault().Get<StandardConfigurationControlsCM>();
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
            return templates.Where(a => a.Name == "MakeADecision").FirstOrDefault();
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
                    var controlsCrate = crateStorage.Where(a => a.ManifestType.Id == (int)Fr8Data.Constants.MT.StandardConfigurationControls).FirstOrDefault().Get<StandardConfigurationControlsCM>();
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

        // Commented out by yakov.gnusin,
        // since we're using new approach described in https://maginot.atlassian.net/wiki/display/SH/Plan+Directory+Architecture

        // public PlanTemplateDTO GetTemplate(int planDescriptionId, string userId)
        // {
        //     using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //     {
        //         var planDescription = uow.PlanTemplateRepository.GetQuery()
        //             .Include(t => t.PlanNodeDescriptions.Select(x => x.ActivityDescription))
        //             .Include(t => t.PlanNodeDescriptions.Select(x => x.Transitions))
        //             .Where(a => a.Id == planDescriptionId && a.User.Id == userId).FirstOrDefault();
        // 
        //         return Mapper.Map<PlanTemplateDTO>(planDescription);
        //     }
        // }
        // 
        // public List<PlanTemplateDTO> GetTemplates(string userId)
        // {
        //     using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //     {
        //         return uow.PlanTemplateRepository.GetQuery().Where(a => a.Fr8AccountId == userId).ToList().Select(b => Mapper.Map<PlanTemplateDTO>(b)).ToList();
        //     }
        // }
        // 
        // public void DeleteTemplate(int id, string userId)
        // {
        //     using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //     {
        //         var templatesForIntegrationUser = uow.PlanTemplateRepository.GetQuery().Where(a => a.User.Id == userId).ToList();
        //         templatesForIntegrationUser.ForEach(a => uow.PlanTemplateRepository.Remove(a));
        //         uow.SaveChanges();
        //     }
        // }
        // 
        // public PlanTemplateDTO SavePlan(Guid planId, string curFr8UserId)
        // {
        //     using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //     {
        //         var plan = uow.PlanRepository.GetById<PlanDO>(planId);
        // 
        //         var all_activities = plan.GetDescendants().OfType<ActivityDO>().ToList();
        //         string all_activities_templates = string.Join(",", all_activities.Select(a => a.ActivityTemplateId));
        //         var related_templates = uow.ActivityTemplateRepository.GetQuery().Where(a => all_activities_templates.Contains(a.Id.ToString())).ToList();
        //         Dictionary<Guid, PlanNodeDescriptionDO> nodes_dictionary = new Dictionary<Guid, PlanNodeDescriptionDO>();
        // 
        //         var planDescription = new PlanTemplateDO();
        //         var descriptions = uow.PlanTemplateRepository.GetQuery().Where(a => a.Fr8AccountId == curFr8UserId &&
        //         a.Name.Contains(plan.Name)).ToList() //linq to entities
        //         .Where(a => (a.Name == plan.Name) || Regex.Match(a.Name, "^(" + plan.Name + " #\\d*)").Success).ToList(); //linq
        //         string name = CalculateName(curFr8UserId, descriptions, plan);
        // 
        //         planDescription.Name = name;
        //         planDescription.Description = plan.Description;
        //         planDescription.Fr8AccountId = curFr8UserId;
        //         planDescription.PlanNodeDescriptions = new List<PlanNodeDescriptionDO>();
        //         uow.PlanTemplateRepository.Add(planDescription);
        // 
        //         var all_nodes_dictionary = new Dictionary<Guid, PlanNodeDescriptionDO>();
        //         foreach (var subplan in plan.SubPlans.Where(a => a.ChildNodes.Count > 0))
        //         {
        //             BuildPlanTemplateNodes(subplan, planDescription, all_activities, related_templates, plan.StartingSubPlanId == subplan.Id, ref all_nodes_dictionary);
        //         }
        // 
        //         BuildJumpTransitions(all_nodes_dictionary, all_activities, related_templates, planDescription, plan);
        // 
        //         uow.SaveChanges();
        // 
        //         return Mapper.Map<PlanTemplateDTO>(planDescription);
        //     }
        // }
        // 
        // public string LoadPlan(int planDescriptionId, string userId)
        // {
        //     using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //     {
        //         var planDescription = uow.PlanTemplateRepository.GetQuery()
        //             .Include(t => t.PlanNodeDescriptions.Select(x => x.ActivityDescription))
        //             .Include(t => t.PlanNodeDescriptions.Select(x => x.Transitions))
        //             .Where(a => a.Id == planDescriptionId).FirstOrDefault();
        // 
        //         var planDO = new PlanDO() { Fr8AccountId = userId, Name = planDescription.Name + " — from a plan description", PlanState = 1, ChildNodes = new List<PlanNodeDO>(), Description = planDescription.Description };
        // 
        //         var allNodesDictionary = new Dictionary<ActivityDO, PlanNodeDescriptionDO>();
        //         var first_activities = planDescription.PlanNodeDescriptions.Where(a => a.ParentNode == null);
        //         foreach (var startingActivity in first_activities)
        //         {
        //             var subplan = new SubplanDO() { Name = startingActivity.SubPlanName };
        //             if (planDescription.StartingPlanNodeDescription.Id == startingActivity.Id)
        //                 planDO.StartingSubplan = subplan;
        //             else
        //                 planDO.ChildNodes.Add(subplan);
        //             BuildPlanNodes(userId, 1, subplan, planDescription, startingActivity, planDO, ref allNodesDictionary);
        //         }
        // 
        //         uow.PlanRepository.Add(planDO);
        // 
        //         UpdateJumpTransitions(planDO, planDescription, allNodesDictionary);
        // 
        //         uow.SaveChanges();
        // 
        //         return planDO.Id.ToString();
        //     }
        // }
        // 
        // 
        // #region Saving
        // 
        // private Dictionary<Guid, PlanNodeDescriptionDO> BuildPlanTemplateNodes(PlanNodeDO nodeDO, PlanTemplateDO planDescription, List<ActivityDO> all_activities,
        //    List<ActivityTemplateDO> related_templates, bool startingSubplan, ref Dictionary<Guid, PlanNodeDescriptionDO> allNodesDictionary)
        // {
        //     var result = new Dictionary<Guid, PlanNodeDescriptionDO>();
        //     foreach (var activityNode in nodeDO.ChildNodes.OrderBy(a => a.Ordering))
        //     {
        //         var firstActivity = all_activities.Where(a => a.Id == activityNode.Id).FirstOrDefault();
        //         var template = related_templates.Where(a => a.Id == firstActivity.ActivityTemplateId).FirstOrDefault();
        //         var node = CreatePlanNodeDescription(firstActivity, template);
        // 
        //         if (nodeDO is SubplanDO)
        //         {
        //             node.SubPlanName = (nodeDO as SubplanDO).Name;
        //             node.SubPlanOriginalId = nodeDO.Id.ToString();
        //         }
        // 
        //         if (startingSubplan)
        //         {
        //             planDescription.StartingPlanNodeDescription = node;
        //             startingSubplan = false;
        //         }
        //         else
        //             planDescription.PlanNodeDescriptions.Add(node);
        // 
        //         result.Add(firstActivity.Id, node);
        //         if (!allNodesDictionary.ContainsKey(firstActivity.Id))
        //             allNodesDictionary.Add(firstActivity.Id, node);
        // 
        // 
        //         //Handle children transitions
        //         var child_nodes = BuildPlanTemplateNodes(activityNode, planDescription, all_activities, related_templates, startingSubplan, ref allNodesDictionary);
        //         if (child_nodes.Count > 0)
        //         {
        //             var child_activities = new List<ActivityDO>();
        //             foreach (var activity in all_activities)
        //             {
        //                 if (child_nodes.ContainsKey(activity.Id))
        //                     child_activities.Add(activity);
        //             }
        //             var single_child = child_activities.OrderBy(a => a.Ordering).FirstOrDefault().Id;
        //             var child = child_nodes[single_child];
        //             node.Transitions.Add(new NodeTransitionDO() { ActivityDescription = child.ActivityDescription, Transition = PlanNodeTransitionType.Child });
        //             child.ParentNode = node;
        //             child.SubPlanName = node.SubPlanName;
        //         }
        //     }
        // 
        //     //handle upstream/downstream transitions
        //     foreach (var activityNode in nodeDO.ChildNodes)
        //     {
        //         var current_node = result[activityNode.Id];
        //         var upstream_activity = nodeDO.ChildNodes.Where(a => a.Id != activityNode.Id && a.Ordering == (activityNode.Ordering - 1)).FirstOrDefault();
        //         if (upstream_activity != null)
        //         {
        //             var upstream_node = result[upstream_activity.Id];
        //             upstream_node.Transitions.Add(new NodeTransitionDO() { ActivityDescription = current_node.ActivityDescription, Transition = PlanNodeTransitionType.Downstream });
        //             current_node.ParentNode = upstream_node;
        //         }
        //     }
        // 
        //     return result;
        // }
        // 
        // private string CalculateName(string curFr8UserId, List<PlanTemplateDO> descriptions, PlanDO plan)
        // {
        //     var existingDescriptionWithLargestNumber = descriptions.OrderBy(a => a.Name.Length).ThenBy(b => b.Name).LastOrDefault();
        //     string name = plan.Name;
        //     if (existingDescriptionWithLargestNumber != null)
        //     {
        //         string largestNumber = "0";
        //         if (existingDescriptionWithLargestNumber.Name.Length != plan.Name.Length)
        //         {
        //             largestNumber = existingDescriptionWithLargestNumber.Name.Substring((plan.Name + " #").Length);
        //         }
        //         int number = Convert.ToInt32(largestNumber) + 1;
        //         name += string.Format(" #{0}", number);
        //     }
        //     return name;
        // }
        // 
        // private void BuildJumpTransitions(
        //     Dictionary<Guid, PlanNodeDescriptionDO> allNodesDictionary, 
        //     List<ActivityDO> allActivities, 
        //     List<ActivityTemplateDO> relatedTemplates,
        //     PlanTemplateDO planDescription, 
        //     PlanDO plan)
        // {
        //     var makeADecisionTemplate = FindMakeADecisionTemplate(relatedTemplates.ToList());
        //     if (makeADecisionTemplate == null) return;
        // 
        //     var makeADecisionActivities = allActivities.Where(a => a.ActivityTemplateId == makeADecisionTemplate.Id).ToList();
        // 
        //     foreach (var tabActivity in makeADecisionActivities)
        //     {
        //         var makeADecisionNode = allNodesDictionary[tabActivity.Id];
        //         using (var crateStorage = _crateManager.GetUpdatableStorage(tabActivity))
        //         {
        //             var controlsCrate = crateStorage.Where(a => a.ManifestType.Id == (int)Fr8Data.Constants.MT.StandardConfigurationControls).FirstOrDefault().Get<StandardConfigurationControlsCM>();
        //             var transitionsPanel = (ContainerTransition)controlsCrate.Controls.Where(a => a.Type == ControlTypes.ContainerTransition).FirstOrDefault();
        // 
        //             foreach (var transition in transitionsPanel.Transitions.Where(a => a.TargetNodeId.HasValue))
        //             {
        //                 switch (transition.Transition)
        //                 {
        //                     case ContainerTransitions.JumpToActivity:
        //                         var target_node = allNodesDictionary[transition.TargetNodeId.Value];
        //                         makeADecisionNode.Transitions.Add(new NodeTransitionDO() { ActivityDescription = target_node.ActivityDescription, Transition = PlanNodeTransitionType.Jump });
        //                         break;
        // 
        //                     case ContainerTransitions.JumpToSubplan:
        //                         var subplan = plan.SubPlans.Where(a => a.Id == transition.TargetNodeId.Value).FirstOrDefault();
        //                         var target_starting_nodeDO = subplan.ChildNodes.Where(a => a.Ordering == 1).FirstOrDefault();
        // 
        //                         if (target_starting_nodeDO != null) //false if a jump is targeting an emty subplan, so we won't create transitions
        //                         {
        //                             var target_starting_node = allNodesDictionary[target_starting_nodeDO.Id];
        //                             makeADecisionNode.Transitions.Add(new NodeTransitionDO() { ActivityDescription = target_starting_node.ActivityDescription, Transition = PlanNodeTransitionType.Jump });
        //                         }
        //                         break;
        // 
        //                     case ContainerTransitions.LaunchAdditionalPlan:
        //                         // for now let's jump to an existing a plan, but that will lead to "unsharable" plan description,
        //                         // so in future we might want to evaluate if a user wants a target plan to be saved as description as well
        //                         makeADecisionNode.Transitions.Add(new NodeTransitionDO() { PlanId = transition.TargetNodeId, Transition = PlanNodeTransitionType.Jump });
        //                         break;
        //                 }
        //             }
        //         }
        // 
        //     }
        // }
        // 
        // private PlanNodeDescriptionDO CreatePlanNodeDescription(ActivityDO firstActivity, ActivityTemplateDO template)
        // {
        //     PlanNodeDescriptionDO planNode = new PlanNodeDescriptionDO();
        //     planNode.Name = firstActivity.Label;
        //     planNode.ActivityDescription = new ActivityDescriptionDO()
        //     {
        //         ActivityTemplateId = firstActivity.ActivityTemplateId,
        //         OriginalId = firstActivity.Id.ToString(),
        //         Name = firstActivity.Label,
        //         Version = template.Version,
        //         CrateStorage = firstActivity.CrateStorage
        //     };
        //     planNode.Transitions = new List<NodeTransitionDO>();
        //     return planNode;
        // }
        // 
        // #endregion
        // 
        // #region Loading
        // 
        // private void UpdateJumpTransitions(PlanDO planDO, PlanTemplateDO planDescription, Dictionary<ActivityDO, PlanNodeDescriptionDO> allNodesDictionary)
        // {
        //     //get TAB activities
        //     var makeADecisionActivities = new List<ActivityDO>();
        //     var all_activitiesTemplatesIds = string.Join(",", allNodesDictionary.Select(a => a.Value).Select(b => b.ActivityDescription).Select(c => c.ActivityTemplateId).ToArray());
        //     var used_templates = new List<ActivityTemplateDO>();
        //     using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
        //     {
        //         used_templates = uow.ActivityTemplateRepository.GetQuery().Where(a => all_activitiesTemplatesIds.Contains(a.Id.ToString())).ToList();
        //         var makeADecisionTemplate = FindMakeADecisionTemplate(used_templates);
        //         if (makeADecisionTemplate == null) return;
        //         var all_used_activities = GetAllPlanActivities(planDO);
        //         makeADecisionActivities = all_used_activities.Where(a => a.ActivityTemplateId == makeADecisionTemplate.Id).ToList();
        //         if (makeADecisionActivities.Count == 0) return;
        //     }
        // 
        //     //process
        //     foreach (var makeADecisionActivity in makeADecisionActivities)
        //     {
        //         using (var crateStorage = _crateManager.GetUpdatableStorage(makeADecisionActivity))
        //         {
        //             var controlsCrate = crateStorage.Where(a => a.ManifestType.Id == (int)Fr8Data.Constants.MT.StandardConfigurationControls).FirstOrDefault().Get<StandardConfigurationControlsCM>();
        //             var transitionsPanel = (ContainerTransition)controlsCrate.Controls.Where(a => a.Type == ControlTypes.ContainerTransition).FirstOrDefault();
        // 
        //             foreach (var transition in transitionsPanel.Transitions)
        //             {
        //                 if (transition.TargetNodeId != null)
        //                 {
        //                     switch (transition.Transition)
        //                     {
        //                         case ContainerTransitions.JumpToActivity:
        //                             var targetActivityNode = planDescription.PlanNodeDescriptions.Where(a => a.ActivityDescription.OriginalId == transition.TargetNodeId.ToString()).FirstOrDefault();
        //                             var targetActivity = allNodesDictionary.Where(a => a.Value == targetActivityNode).FirstOrDefault().Key;
        //                             transition.TargetNodeId = targetActivity.Id;
        //                             break;
        //                         case ContainerTransitions.JumpToSubplan:
        //                             var targetStartingActivityNode = planDescription.PlanNodeDescriptions.Where(a => a.SubPlanOriginalId == transition.TargetNodeId.ToString()).FirstOrDefault();
        //                             var startingActivity = allNodesDictionary.Where(a => a.Value == targetStartingActivityNode).FirstOrDefault().Key;
        //                             transition.TargetNodeId = startingActivity.ParentPlanNode.Id;
        //                             break;
        //                         case ContainerTransitions.LaunchAdditionalPlan:
        //                             break;
        //                     }
        //                 }
        //             }
        //         }
        //     }
        // }
        // 
        // private List<ActivityDO> GetAllPlanActivities(PlanNodeDO planNode)
        // {
        //     var result = new List<ActivityDO>();
        //     foreach (var child in planNode.ChildNodes)
        //     {
        //         if (child is ActivityDO)
        //             result.Add((ActivityDO)child);
        //         var childrenNodes = GetAllPlanActivities(child);
        //         result.AddRange(childrenNodes);
        //     }
        // 
        //     return result;
        // }
        // 
        // private void BuildPlanNodes(string userId, int ordering, PlanNodeDO planNodeDO, PlanTemplateDO planDescription, PlanNodeDescriptionDO parentNode, PlanDO planDO, ref Dictionary<ActivityDO, PlanNodeDescriptionDO> allNodesDictionary)
        // {
        //     ActivityDO parentNodeActivity = CreateActivityDO(userId, ordering, planNodeDO, parentNode, planDO);
        //     planNodeDO.ChildNodes.Add(parentNodeActivity);
        //     allNodesDictionary.Add(parentNodeActivity, parentNode);
        // 
        //     var child_nodes = planDescription.PlanNodeDescriptions.Where(a => a.ParentNode == parentNode);
        //     foreach (var child in child_nodes)
        //     {
        //         var transition = parentNode.Transitions.Where(a => a.ActivityDescriptionId == child.ActivityDescription.Id).FirstOrDefault().Transition;
        //         if (transition == PlanNodeTransitionType.Downstream)
        //         {
        //             BuildPlanNodes(userId, ++ordering, planNodeDO, planDescription, child, planDO, ref allNodesDictionary);
        //         }
        //         if (transition == PlanNodeTransitionType.Child)
        //         {
        //             BuildPlanNodes(userId, ordering, parentNodeActivity, planDescription, child, planDO, ref allNodesDictionary);
        //         }
        //     }
        // }
        // 
        // private ActivityDO CreateActivityDO(string userId, int ordering, PlanNodeDO planNodeDO, PlanNodeDescriptionDO parentNode, PlanDO planDO)
        // {
        //     var activityDO = new ActivityDO();
        //     activityDO.CrateStorage = parentNode.ActivityDescription.CrateStorage;
        //     activityDO.ActivityTemplateId = parentNode.ActivityDescription.ActivityTemplateId;
        //     activityDO.Fr8AccountId = userId;
        //     activityDO.Label = parentNode.Name;
        //     activityDO.Ordering = ordering;
        //     activityDO.ParentPlanNode = planNodeDO;
        //     activityDO.RootPlanNode = planDO;
        //     return activityDO;
        // }
        // #endregion
        // 
        // private ActivityTemplateDO FindMakeADecisionTemplate(List<ActivityTemplateDO> templates)
        // {
        //     return templates.Where(a => a.Name == "MakeADecision").FirstOrDefault();
        // }

    }
}
