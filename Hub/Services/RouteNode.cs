using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Data.Constants;
using Data.Crates;
using Newtonsoft.Json;
using StructureMap;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;

using Hub.Interfaces;
using Hub.Managers;
using Utilities.Configuration.Azure;
using Hub.Managers.APIManagers.Transmitters.Restful;
using Data.Interfaces.Manifests;

namespace Hub.Services
{
    public class RouteNode : IRouteNode
    {
        #region Fields

        private readonly ICrateManager _crate;
        private readonly IRestfulServiceClient _restfulServiceClient;
        private readonly IRouteNode _activity;
        private readonly IActivityTemplate _activityTemplate;
        #endregion

        public RouteNode()
        {
            _activityTemplate = ObjectFactory.GetInstance<IActivityTemplate>();
            _crate = ObjectFactory.GetInstance<ICrateManager>();
            _restfulServiceClient = ObjectFactory.GetInstance<IRestfulServiceClient>();
        }

        //This builds a list of an activity and all of its descendants, over multiple levels
        // public List<RouteNodeDO> GetActivityTree(IUnitOfWork uow, RouteNodeDO curActivity)
        // {
        //     var curList = new List<RouteNodeDO>();
        //     curList.Add(curActivity);
        //     var childActivities = GetChildren(uow, curActivity);
        //     foreach (var child in childActivities)
        //     {
        //         curList.AddRange(GetActivityTree(uow, child));
        //     }
        //     return curList;
        // }

        public List<RouteNodeDO> GetActivityTree(IEnumerable<RouteNodeDO> fullTree, RouteNodeDO curActivity)
        {
            var curList = new List<RouteNodeDO>();
            curList.Add(curActivity);

            // var childActivities = GetChildren(uow, curActivity);
            var childActivities = fullTree
                .Where(x => x.ParentRouteNodeId == curActivity.Id)
                .OrderBy(z => z.Ordering);

            foreach (var child in childActivities)
            {
                curList.AddRange(GetActivityTree(fullTree, child));
            }

            return curList;
        }

        public List<RouteNodeDO> GetUpstreamActivities(IUnitOfWork uow, RouteNodeDO curActivityDO)
        {
            if (curActivityDO == null)
                throw new ArgumentNullException("curActivityDO");
            if (curActivityDO.ParentRouteNodeId == null)
                return new List<RouteNodeDO>();

            var fullTree = uow.RouteNodeRepository.GetAll()
                .Where(x => x.RootRouteNodeId == curActivityDO.RootRouteNodeId)
                .ToList();

            return GetUpstreamActivities(fullTree, curActivityDO);
        }

        public StandardDesignTimeFieldsCM GetDesignTimeFieldsByDirection(Guid activityId, CrateDirection direction, AvailabilityType availability)
        {
            StandardDesignTimeFieldsCM mergedFields = new StandardDesignTimeFieldsCM();

            Func<FieldDTO, bool> fieldPredicate;
            if (availability == AvailabilityType.NotSet)
            {
                fieldPredicate = (FieldDTO f) => true;
            }
            else
            {
                fieldPredicate = (FieldDTO f) => f.Availability == availability;
            }

            Func<Crate<StandardDesignTimeFieldsCM>, bool> cratePredicate;
            if (availability == AvailabilityType.NotSet)
            {
                cratePredicate = (Crate<StandardDesignTimeFieldsCM> f) => true;
            }
            else
            {
                cratePredicate = (Crate<StandardDesignTimeFieldsCM> f) =>
                {
                    return f.Availability == availability;
                };
            }

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                ActionDO actionDO = uow.ActionRepository.GetByKey(activityId);
                var curCrates = GetActivitiesByDirection(uow, direction, actionDO)
                    .OfType<ActionDO>()
                    .SelectMany(x => _crate.GetStorage(x).CratesOfType<StandardDesignTimeFieldsCM>().Where(cratePredicate))
                    .ToList();

                mergedFields.Fields.AddRange(_crate.MergeContentFields(curCrates).Fields.Where(fieldPredicate));
                return mergedFields;
            }
        }

        private List<RouteNodeDO> GetActivitiesByDirection(IUnitOfWork uow, CrateDirection direction, RouteNodeDO curActivityDO)
        {
            switch (direction)
            {
                case CrateDirection.Downstream:
                    return GetDownstreamActivities(uow, curActivityDO);
                case CrateDirection.Upstream:
                    return GetUpstreamActivities(uow, curActivityDO);
                case CrateDirection.Both:
                default:
                    return  GetDownstreamActivities(uow, curActivityDO).Concat(GetUpstreamActivities(uow, curActivityDO)).ToList();
            }
        }

        private List<RouteNodeDO> GetUpstreamActivities(
            IEnumerable<RouteNodeDO> fullTree,
            RouteNodeDO curActivityDO)
        {
            if (curActivityDO == null)
            {
                throw new ArgumentNullException("curActivityDO");
            }

            if (curActivityDO.ParentRouteNodeId == null)
            {
                return new List<RouteNodeDO>();
            }

            var upstreamActivities = new List<RouteNodeDO>();

            //start by getting the parent of the current action
            // var parentActivity = uow.RouteNodeRepository.GetByKey(curActivityDO.ParentRouteNodeId);
            var parentActivity = fullTree.Single(x => x.Id == curActivityDO.ParentRouteNodeId);


            // find all sibling actions that have a lower Ordering. These are the ones that are "above" this action in the list
            // var upstreamSiblings =
            //     parentActivity.ChildNodes.Where(a => a.Ordering < curActivityDO.Ordering);
            var upstreamSiblings =
                fullTree.Where(a => a.ParentRouteNodeId == curActivityDO.ParentRouteNodeId
                    && a.Ordering < curActivityDO.Ordering);

            //for each such sibling action, we want to add it to the list
            //but some of those activities may be actionlists with childactivities of their own
            //in that case we need to recurse
            foreach (var upstreamSibling in upstreamSiblings)
            {
                //1) first add the upstream siblings
                upstreamActivities.AddRange(GetActivityTree(fullTree, upstreamSibling));
            }

            //now we need to recurse up to the parent of the current activity, and repeat until we reach the root of the tree
            if (parentActivity != null)
            {
                //2) then add the parent activity...
                upstreamActivities.Add(parentActivity);
                //3) then add the parent's upstream activities
                upstreamActivities.AddRange(GetUpstreamActivities(fullTree, parentActivity));
            }
            else return upstreamActivities;

            return upstreamActivities; //should never actually get here, but the compiler insists
        }

        public List<RouteNodeDO> GetDownstreamActivities(IUnitOfWork uow, RouteNodeDO curActivityDO)
        {
            if (curActivityDO == null)
                throw new ArgumentNullException("curActivity");
            if (curActivityDO.ParentRouteNodeId == null)
                return new List<RouteNodeDO>();

            var fullTree = uow.RouteNodeRepository.GetAll()
                .Where(x => x.RootRouteNodeId == curActivityDO.RootRouteNodeId)
                .ToList();

            return GetDownstreamActivities(fullTree, curActivityDO);
        }

        private List<RouteNodeDO> GetDownstreamActivities(
            IEnumerable<RouteNodeDO> fullTree,
            RouteNodeDO curActivityDO)
        {
            if (curActivityDO == null)
            {
                throw new ArgumentNullException("curActivityDO");
            }

            if (curActivityDO.ParentRouteNodeId == null)
            {
                return new List<RouteNodeDO>();
            }

            var downstreamList = new List<RouteNodeDO>();

            //start by getting the parent of the current action
            // var parentActivity = uow.RouteNodeRepository.GetByKey(curActivity.ParentRouteNodeId);
            var parentActivity = fullTree.Single(x => x.Id == curActivityDO.ParentRouteNodeId);

            // find all sibling actions that have a higher Ordering. These are the ones that are "below" or downstream of this action in the list
            // var downstreamSiblings = parentActivity.ChildNodes.Where(a => a.Ordering > curActivity.Ordering);
            var downstreamSiblings =
                fullTree.Where(a => a.ParentRouteNodeId == curActivityDO.ParentRouteNodeId
                    && a.Ordering > curActivityDO.Ordering);

            //for each such sibling action, we want to add it to the list
            //but some of those activities may be actionlists with childactivities of their own
            //in that case we need to recurse
            foreach (var downstreamSibling in downstreamSiblings)
            {
                //1) first add the downstream siblings and their descendants
                downstreamList.AddRange(GetActivityTree(fullTree, downstreamSibling));
            }

            //now we need to recurse up to the parent of the current activity, and repeat until we reach the root of the tree
            if (parentActivity != null)
            {
                //find the downstream siblings of the parent activity and add them and their descendants

                downstreamList.AddRange(GetDownstreamActivities(fullTree, parentActivity));
            }

            return downstreamList;
        }

        public RouteNodeDO GetParent(RouteNodeDO currentActivity)
        {
            return currentActivity.ParentRouteNode;
        }

        public RouteNodeDO GetNextSibling(RouteNodeDO currentActivity)
        {
            // Move to the next activity of the current activity's parent
            if (currentActivity.ParentRouteNode == null)
            {
                // We are at the root of activity tree. Next activity can be only among children.
                return null;
            }

            return currentActivity.ParentRouteNode.ChildNodes
                .OrderBy(x => x.Ordering)
                .FirstOrDefault(x => x.Ordering > currentActivity.Ordering);
        }

        public RouteNodeDO GetFirstChild(RouteNodeDO currentActivity)
        {
            if (currentActivity.ChildNodes.Count != 0)
            {
                return currentActivity.ChildNodes.OrderBy(x => x.Ordering).FirstOrDefault();
            }

            return null;
        }

        public RouteNodeDO GetNextActivity(RouteNodeDO currentActivity, RouteNodeDO root)
        {
            return GetNextActivity(currentActivity, true, root);
        }

        public bool HasChildren(RouteNodeDO currentActivity)
        {
            return currentActivity.ChildNodes.Count > 0;
        }

        private RouteNodeDO GetNextActivity(RouteNodeDO currentActivity, bool depthFirst, RouteNodeDO root)
        {
            // Move to the first child if current activity has nested ones
            if (depthFirst && currentActivity.ChildNodes.Count != 0)
            {
                return currentActivity.ChildNodes.OrderBy(x => x.Ordering).FirstOrDefault();
            }

            // Move to the next activity of the current activity's parent
            if (currentActivity.ParentRouteNode == null)
            {
                // We are at the root of activity tree. Next activity can be only among children.
                return null;
            }

            var prev = currentActivity;
            var nextCandidate = currentActivity.ParentRouteNode.ChildNodes
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
                nextCandidate = GetNextActivity(prev.ParentRouteNode, false, root);
            }

            return nextCandidate;
        }

        public void Delete(IUnitOfWork uow, RouteNodeDO activity)
        {
            var activities = new List<RouteNodeDO>();

            TraverseActivity(activity, activities.Add);

	        activities.Reverse();

            activities.ForEach(x =>
            {
                // TODO: it is not very smart solution. Activity service should not knon about anything except Activities
                // But we have to support correct deletion of any activity types and any level of hierarchy
                // May be other services should register some kind of callback to get notifed when activity is being deleted.
                if (x is SubrouteDO)
                {
                    foreach (var criteria in uow.CriteriaRepository.GetQuery().Where(y => y.SubrouteId == x.Id).ToArray())
                    {
                        uow.CriteriaRepository.Remove(criteria);
                    }
                }

                uow.RouteNodeRepository.Remove(x);
            });
        }

        private static void TraverseActivity(RouteNodeDO parent, Action<RouteNodeDO> visitAction)
        {
            visitAction(parent);
            foreach (RouteNodeDO child in parent.ChildNodes)
                TraverseActivity(child, visitAction);
        }

        private IEnumerable<RouteNodeDO> GetChildren(IUnitOfWork uow, RouteNodeDO currActivity)
        {
            // Get all activities which parent is currActivity and order their by Ordering. The order is important!
            var orderedActivities = uow.RouteNodeRepository.GetAll()
            .Where(x => x.ParentRouteNodeId == currActivity.Id)
            .OrderBy(z => z.Ordering);
            return orderedActivities;
        }



        public async Task Process(Guid curActivityId, ActionState curActionState, ContainerDO containerDO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //why do we get container from db again???
                var curContainerDO = uow.ContainerRepository.GetByKey(containerDO.Id);
                var curActivityDO = uow.RouteNodeRepository.GetByKey(curActivityId);

                if (curActivityDO == null)
                {
                    throw new ArgumentException("Cannot find Activity with the supplied curActivityId");
                }

                if (curActivityDO is ActionDO)
                {
                    IAction _action = ObjectFactory.GetInstance<IAction>();
                    await _action.PrepareToExecute((ActionDO)curActivityDO, curActionState, curContainerDO, uow);
                    //TODO inspect this
                    //why do we get container from db again???
                    containerDO.CrateStorage = curContainerDO.CrateStorage;
                }
            }
        }

        public IEnumerable<ActivityTemplateDTO> GetAvailableActivities(IUnitOfWork uow, IFr8AccountDO curAccount)
        {
            IEnumerable<ActivityTemplateDTO> curActivityTemplates;

            curActivityTemplates = _activityTemplate
                .GetAll()
                .OrderBy(t => t.Category)
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
                .OrderBy(t => t.Category)
                .Select(Mapper.Map<ActivityTemplateDTO>)
                .ToList();
        }

        public IEnumerable<ActivityTemplateDTO> GetSolutions(IUnitOfWork uow, IFr8AccountDO curAccount)
        {
            IEnumerable<ActivityTemplateDTO> curActivityTemplates;
            curActivityTemplates = _activityTemplate
                .GetAll()
                .Where(at => at.Category == Data.States.ActivityCategory.Solution 
                    && at.ActivityTemplateState == Data.States.ActivityTemplateState.Active)
                .OrderBy(t => t.Category)
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

        public IEnumerable<ActivityTemplateCategoryDTO> GetAvailableActivitiyGroups()
        {
            var curActivityTemplates = _activityTemplate
                .GetQuery()
                .Where(at => at.ActivityTemplateState == ActivityTemplateState.Active).AsEnumerable().ToArray()
                .GroupBy(t => t.Category)
                .OrderBy(c => c.Key)
                .Select(c => new ActivityTemplateCategoryDTO
                {
                    Activities = c.Select(Mapper.Map<ActivityTemplateDTO>).ToList(),
                    Name = c.Key.ToString()
                })
                .ToList();


            return curActivityTemplates;
        }

    }
}