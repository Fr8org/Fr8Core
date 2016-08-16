using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Fr8.Infrastructure.Data.Crates;
using Fr8.Infrastructure.Data.DataTransferObjects;
using Fr8.Infrastructure.Data.Managers;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.Infrastructure.Data.States;
using Hub.Interfaces;
using Hub.Managers;


namespace Hub.Services
{
    public class PlanNode : IPlanNode
    {
        #region ActivityCategories comparer.

        private class ActivitiesCategoriesComparer : IComparer<IEnumerable<ActivityCategorySetDO>>
        {
            public int Compare(IEnumerable<ActivityCategorySetDO> x, IEnumerable<ActivityCategorySetDO> y)
            {
                var xIndex = GetActivityCategoryIndex(x);
                var yIndex = GetActivityCategoryIndex(y);

                return xIndex.CompareTo(yIndex);
            }

            private static int GetActivityCategoryIndex(IEnumerable<ActivityCategorySetDO> s)
            {
                for (int i = 0; i < ActivityCategories.ActivityCategoryIds.Count; ++i)
                {
                    if (s.Select(x => x.ActivityCategoryId).Contains(ActivityCategories.ActivityCategoryIds[i]))
                    {
                        return i + 1;
                    }
                }

                return ActivityCategories.ActivityCategoryIds.Count + 1;
            }
        }

        #endregion ActivityCategories comparer.

        #region Fields

        private readonly ICrateManager _crate;
        private readonly ITerminal _terminal;
        private readonly IActivityTemplate _activityTemplate;
        private const string ValidationErrorsLabel = "Validation Errors";

        #endregion

        public PlanNode(ICrateManager crateManager, IActivityTemplate activityTemplate, ITerminal terminal)
        {
            _terminal = terminal;
            _activityTemplate = activityTemplate;
            _crate = crateManager;
        }

        public List<PlanNodeDO> GetUpstreamActivities(IUnitOfWork uow, PlanNodeDO curActivityDO)
        {
            if (curActivityDO == null)
                throw new ArgumentNullException("curActivityDO");
            if (curActivityDO.ParentPlanNodeId == null)
                return new List<PlanNodeDO>();

            List<PlanNodeDO> planNodes = new List<PlanNodeDO>();
            var node = curActivityDO;

            do
            {
                var currentNode = node;

                if (node.ParentPlanNode != null)
                {
                    foreach (var predcessors in node.ParentPlanNode.ChildNodes.Where(x => x.Ordering < currentNode.Ordering && x != currentNode).OrderByDescending(x => x.Ordering))
                    {
                        GetDownstreamRecusive(predcessors, planNodes);
                    }
                }

                node = node.ParentPlanNode;

                if (node != null)
                {
                    planNodes.Add(node);
                }

            } while (node != null);

            return planNodes;
        }

        private void GetDownstreamRecusive(PlanNodeDO root, List<PlanNodeDO> items)
        {
            items.Add(root);

            foreach (var child in root.ChildNodes.OrderBy(x => x.Ordering))
            {
                GetDownstreamRecusive(child, items);
            }
        }

        public IncomingCratesDTO GetIncomingData(Guid activityId, CrateDirection direction, AvailabilityType availability)
        {
            var crates = GetCrateManifestsByDirection(activityId, direction, AvailabilityType.NotSet);
            var availableData = new IncomingCratesDTO();
            availableData.AvailableCrates.AddRange(crates.SelectMany(x => x.CrateDescriptions).Where(x => availability == AvailabilityType.NotSet || (x.Availability & availability) != 0));

            return availableData;
        }

        public List<CrateDescriptionCM> GetCrateManifestsByDirection(
            Guid activityId,
            CrateDirection direction,
            AvailabilityType availability,
            bool includeCratesFromActivity = true
                )
        {
            Func<Crate<CrateDescriptionCM>, bool> cratePredicate;


            //validation errors don't need to be present as available data, so remove Validation Errors
            cratePredicate = f => f.Label != ValidationErrorsLabel;


            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var activityDO = uow.PlanRepository.GetById<ActivityDO>(activityId);
                var activities = GetActivitiesByDirection(uow, direction, activityDO)
                    .OfType<ActivityDO>();

                if (!includeCratesFromActivity)
                {
                    activities = activities.Where(x => x.Id != activityId);
                }
                // else
                // {
                //     if (!activities.Any(x => x.Id == activityId))
                //     {
                //         var activitiesToAdd = activities.ToList();
                //         activitiesToAdd.Insert(0, activityDO);
                //         activities = activitiesToAdd;
                //     }
                // }

                var result = activities
                    .SelectMany(x => _crate.GetStorage(x).CratesOfType<CrateDescriptionCM>().Where(cratePredicate))
                    .Select(x =>
                    {
                        foreach (var crateDescription in x.Content.CrateDescriptions)
                        {
                            crateDescription.Label = crateDescription.Label ?? crateDescription.ProducedBy;
                        }
                        return x.Content;
                    })
                    .ToList();

                return result;
            }
        }

        private List<PlanNodeDO> GetActivitiesByDirection(IUnitOfWork uow, CrateDirection direction, PlanNodeDO curActivityDO)
        {
            switch (direction)
            {
                case CrateDirection.Downstream:
                    return GetDownstreamActivities(uow, curActivityDO);

                case CrateDirection.Upstream:
                    return GetUpstreamActivities(uow, curActivityDO);

                default:
                    return GetDownstreamActivities(uow, curActivityDO).Concat(GetUpstreamActivities(uow, curActivityDO)).ToList();
            }
        }

        public List<PlanNodeDO> GetDownstreamActivities(IUnitOfWork uow, PlanNodeDO curActivityDO)
        {
            if (curActivityDO == null)
                throw new ArgumentNullException("curActivity");
            if (curActivityDO.ParentPlanNodeId == null)
                return new List<PlanNodeDO>();

            List<PlanNodeDO> nodes = new List<PlanNodeDO>();

            foreach (var planNodeDo in curActivityDO.ChildNodes)
            {
                GetDownstreamRecusive(planNodeDo, nodes);
            }

            while (curActivityDO != null)
            {
                if (curActivityDO.ParentPlanNode != null)
                {
                    foreach (var sibling in curActivityDO.ParentPlanNode.ChildNodes.Where(x => x.Ordering > curActivityDO.Ordering))
                    {
                        GetDownstreamRecusive(sibling, nodes);
                    }
                }

                curActivityDO = curActivityDO.ParentPlanNode;
            }

            return nodes;
        }

        public PlanNodeDO GetParent(PlanNodeDO currentActivity)
        {
            return currentActivity.ParentPlanNode;
        }

        public PlanNodeDO GetNextSibling(PlanNodeDO currentActivity)
        {
            // Move to the next activity of the current activity's parent
            if (currentActivity.ParentPlanNode == null)
            {
                // We are at the root of activity tree. Next activity can be only among children.
                return null;
            }

            return currentActivity.ParentPlanNode.GetOrderedChildren().FirstOrDefault(x => x.Ordering > currentActivity.Ordering);
        }

        public PlanNodeDO GetFirstChild(PlanNodeDO currentActivity)
        {
            if (currentActivity.ChildNodes.Count != 0)
            {
                return currentActivity.ChildNodes.OrderBy(x => x.Ordering).FirstOrDefault();
            }

            return null;
        }

        public PlanNodeDO GetNextActivity(PlanNodeDO currentActivity, PlanNodeDO root)
        {
            return GetNextActivity(currentActivity, true, root);
        }

        public bool HasChildren(PlanNodeDO currentActivity)
        {
            return currentActivity.ChildNodes.Count > 0;
        }

        private PlanNodeDO GetNextActivity(PlanNodeDO currentActivity, bool depthFirst, PlanNodeDO root)
        {
            // Move to the first child if current activity has nested ones
            if (depthFirst && currentActivity.ChildNodes.Count != 0)
            {
                return currentActivity.ChildNodes.OrderBy(x => x.Ordering).FirstOrDefault();
            }

            // Move to the next activity of the current activity's parent
            if (currentActivity.ParentPlanNode == null)
            {
                // We are at the root of activity tree. Next activity can be only among children.
                return null;
            }

            var prev = currentActivity;
            var nextCandidate = currentActivity.ParentPlanNode.ChildNodes
                .OrderBy(x => x.Ordering)
                .FirstOrDefault(x => x.Ordering > currentActivity.Ordering);

            /* No more activities in the current branch
                *          a
                *       b     c 
                *     d   E  f  g  
                * 
                * We are at E. Get next activity as if current activity is b. (move to c)
                */

            if (nextCandidate == null)
            {
                // Someone doesn't want us to go higher this node
                if (prev == root)
                {
                    return null;
                }
                nextCandidate = GetNextActivity(prev.ParentPlanNode, false, root);
            }

            return nextCandidate;
        }

        public void Delete(IUnitOfWork uow, PlanNodeDO activity)
        {
            var activities = new List<PlanNodeDO>();

            TraverseActivity(activity, activities.Add);

            activities.Reverse();
            activity.RemoveFromParent();
        }

        private static void TraverseActivity(PlanNodeDO parent, Action<PlanNodeDO> visitAction)
        {
            visitAction(parent);
            foreach (PlanNodeDO child in parent.ChildNodes)
                TraverseActivity(child, visitAction);
        }

        public IEnumerable<ActivityTemplateDTO> GetAvailableActivities(IUnitOfWork uow, IFr8AccountDO curAccount)
        {
            IEnumerable<ActivityTemplateDTO> curActivityTemplates;

            curActivityTemplates = _activityTemplate
                .GetAll()
                .OrderBy(t => t.Categories, new ActivitiesCategoriesComparer())
                .Select(Mapper.Map<ActivityTemplateDTO>)
                .ToList();


            //we're currently bypassing the subscription logic until we need it
            //we're bypassing the pluginregistration logic here because it's going away in V2

            //var plugins = _subscription.GetAuthorizedPlugins(curAccount);
            //var plugins = _plugin.GetAll();
            // var curActionTemplates = plugins
            //    .SelectMany(p => p.AvailableActions)
            //    .OrderBy(s => s.ActionType);

            return curActivityTemplates;
        }

        /// <summary>
        /// Returns ActivityTemplates while filtering them by the supplied predicate
        /// </summary>
        public IEnumerable<ActivityTemplateDTO> GetAvailableActivities(IUnitOfWork uow, Func<ActivityTemplateDO, bool> predicate)
        {
            return _activityTemplate
                .GetAll()
                .Where(predicate)
                .Where(at => at.ActivityTemplateState == Data.States.ActivityTemplateState.Active)
                .OrderBy(t => t.Categories, new ActivitiesCategoriesComparer())
                .Select(Mapper.Map<ActivityTemplateDTO>)
                .ToList();
        }

        public IEnumerable<ActivityTemplateDTO> GetSolutions(IUnitOfWork uow)
        {
            var solutionId = ActivityCategories.SolutionId;

            IEnumerable<ActivityTemplateDTO> curActivityTemplates;
            curActivityTemplates = _activityTemplate
                .GetAll()
                .Where(at => at.Categories.Any(y => y.ActivityCategoryId == solutionId)
                    && at.ActivityTemplateState == Data.States.ActivityTemplateState.Active)
                .OrderBy(t => t.Categories, new ActivitiesCategoriesComparer())
                .Select(Mapper.Map<ActivityTemplateDTO>)
                .ToList();

            //we're currently bypassing the subscription logic until we need it
            //we're bypassing the pluginregistration logic here because it's going away in V2

            //var plugins = _subscription.GetAuthorizedPlugins(curAccount);
            //var plugins = _plugin.GetAll();
            // var curActionTemplates = plugins
            //    .SelectMany(p => p.AvailableActions)
            //    .OrderBy(s => s.ActionType);

            return curActivityTemplates;
        }


        public IEnumerable<ActivityTemplateCategoryDTO> GetAvailableActivityGroups()
        {
			var availableTerminalIds = _terminal.GetAll().Select(x=>x.Id).ToList();
            var activityTemplates = _activityTemplate
                .GetQuery()
                .Where(at => at.ActivityTemplateState == ActivityTemplateState.Active
					&& availableTerminalIds.Contains(at.TerminalId)).AsEnumerable().ToArray()
                .ToList();

            var result = ActivityCategories.ActivityCategoryList
                .Where(x => activityTemplates.Any(y => y.Categories.Any(z => z.ActivityCategoryId == x.Id)
                    && y.ActivityTemplateState == ActivityTemplateState.Active))
                .Select(x => new ActivityTemplateCategoryDTO()
                {
                    Name = x.Name,
                    Activities = activityTemplates
                        .Where(y => y.Categories.Any(z => z.ActivityCategoryId == x.Id)
                            && y.ActivityTemplateState == ActivityTemplateState.Active)
                        .GroupBy(y => y.Name)
                        .Select(y => y.OrderByDescending(z => int.Parse(z.Version)).First())
                        .Select(Mapper.Map<ActivityTemplateDTO>)
                        .ToList()
                })
                .ToList();

            return result;
        }

        public IEnumerable<ActivityTemplateCategoryDTO> GetActivityTemplatesGroupedByCategories()
        {
            var availableTerminalIds = _terminal.GetAll().Select(x => x.Id).ToList();

            var categories = _activityTemplate
                .GetQuery()
                .Where(x => availableTerminalIds.Contains(x.TerminalId) && x.Categories != null)
                .SelectMany(x => x.Categories)
                .Select(x => new { x.ActivityCategory.Id, x.ActivityCategory.Name, x.ActivityCategory.IconPath, x.ActivityCategory.Type })
                .OrderBy(x => x.Name)
                .Distinct()
                .ToList();

            var result = categories
                .Select(x => new ActivityTemplateCategoryDTO()
                {
                    Id = x.Id,
                    Name = x.Name,
                    IconPath = x.IconPath,
                    Activities = _activityTemplate.GetQuery()
                        .Where(y => y.Categories != null && y.Categories.Any(z => z.ActivityCategory.Name == x.Name))
                        .GroupBy(y => y.Name)
                        .Select(y => Mapper.Map<ActivityTemplateDTO>(y.OrderByDescending(z => Int32.Parse(z.Version)).First()))
                        .OrderBy(y => y.Label)
                        .ToList()
                })
                .ToList();

            return result;
        }
    }
}