using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Core.Enums;
using Core.Interfaces;
using Core.Managers;
using Data.Entities;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Utilities.Configuration.Azure;
using Newtonsoft.Json;
using StructureMap;

namespace Core.Services
{
	public class RouteNode : IRouteNode
	{
        #region Fields
        
        private readonly ICrateManager _crate;
        
        #endregion

        public RouteNode()
		{
            _crate = ObjectFactory.GetInstance<ICrateManager>();
		}

        //This builds a list of an activity and all of its descendants, over multiple levels
	    public List<RouteNodeDO> GetActivityTree(IUnitOfWork uow, RouteNodeDO curActivity)
	    {
	        var curList = new List<RouteNodeDO>();
	        curList.Add(curActivity);
            var childActivities = GetChildren(uow, curActivity);
	        foreach (var child in childActivities)
	        {
	           curList.AddRange(GetActivityTree(uow, child));
	        }
	        return curList;
	    }

        public List<RouteNodeDO> GetUpstreamActivities(IUnitOfWork uow, RouteNodeDO curActivityDO)
		{
			if (curActivityDO == null)
				throw new ArgumentNullException("curActivityDO");
            if (curActivityDO.ParentRouteNodeId == null)
                return new List<RouteNodeDO>();

			List<RouteNodeDO> upstreamActivities = new List<RouteNodeDO>();

                //start by getting the parent of the current action
                var parentActivity = uow.RouteNodeRepository.GetByKey(curActivityDO.ParentRouteNodeId);


				// find all sibling actions that have a lower Ordering. These are the ones that are "above" this action in the list
				var upstreamSiblings =
				    parentActivity.ChildNodes.Where(a => a.Ordering < curActivityDO.Ordering);
                        
                //for each such sibling action, we want to add it to the list
                //but some of those activities may be actionlists with childactivities of their own
                //in that case we need to recurse
				foreach (var upstreamSibling in upstreamSiblings)
				{
                    //1) first add the upstream siblings
	            upstreamActivities.AddRange(GetActivityTree(uow, upstreamSibling));
				}

                //now we need to recurse up to the parent of the current activity, and repeat until we reach the root of the tree
				if (parentActivity != null)
				{
                    //2) then add the parent activity...
                    upstreamActivities.Add(parentActivity); 
                    //3) then add the parent's upstream activities
				    upstreamActivities.AddRange(GetUpstreamActivities(uow, parentActivity));
				}
				else return upstreamActivities;
					    
            return upstreamActivities; //should never actually get here, but the compiler insists
		}

	    public List<RouteNodeDO> GetDownstreamActivities(IUnitOfWork uow, RouteNodeDO curActivity)
	    {
	        if (curActivity == null)
	            throw new ArgumentNullException("curActivity");
	        if (curActivity.ParentRouteNodeId == null)
	            return new List<RouteNodeDO>();

	        List<RouteNodeDO> downstreamList = new List<RouteNodeDO>();

	        //start by getting the parent of the current action
	        var parentActivity = uow.RouteNodeRepository.GetByKey(curActivity.ParentRouteNodeId);

	        // find all sibling actions that have a higher Ordering. These are the ones that are "below" or downstream of this action in the list
	        var downstreamSiblings = parentActivity.ChildNodes.Where(a => a.Ordering > curActivity.Ordering);

	        //for each such sibling action, we want to add it to the list
	        //but some of those activities may be actionlists with childactivities of their own
	        //in that case we need to recurse
	        foreach (var downstreamSibling in downstreamSiblings)
	        {
	            //1) first add the downstream siblings and their descendants
	            downstreamList.AddRange(GetActivityTree(uow, downstreamSibling));
	        }

	        //now we need to recurse up to the parent of the current activity, and repeat until we reach the root of the tree
	        if (parentActivity != null)
	        {
	            //find the downstream siblings of the parent activity and add them and their descendants

	            downstreamList.AddRange(GetDownstreamActivities(uow, parentActivity));
	        }

	        return downstreamList;
	    }

        public RouteNodeDO GetNextActivity(RouteNodeDO currentActivity, RouteNodeDO root)
		{
	        return GetNextActivity(currentActivity, true, root);
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

        public void Delete (IUnitOfWork uow, RouteNodeDO activity)
        {
            var activities = new List<RouteNodeDO>();

            TraverseActivity(activity, activities.Add);

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

	    

        public async Task Process(int curActivityId, ContainerDO containerDO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curContainerDO = uow.ContainerRepository.GetByKey(containerDO.Id);
                var curActivityDO = uow.RouteNodeRepository.GetByKey(curActivityId);

                if (curActivityDO == null)
                {
                    throw new ArgumentException("Cannot find Activity with the supplied curActivityId");
            }

                if (curActivityDO is ActionDO)
                {
                    IAction _action = ObjectFactory.GetInstance<IAction>();
                    await _action.PrepareToExecute((ActionDO) curActivityDO, curContainerDO, uow);
            }
        }
        }

        public IEnumerable<ActivityTemplateDO> GetAvailableActivities(IUnitOfWork uow, IFr8AccountDO curAccount)
        {
            List<ActivityTemplateDO> curActivityTemplates;

                curActivityTemplates = uow.ActivityTemplateRepository.GetAll().OrderBy(t => t.Category).ToList();
        

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
            List<ActivityTemplateCategoryDTO> curActivityTemplates;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curActivityTemplates = uow.ActivityTemplateRepository.GetAll()
                    .GroupBy(t => t.Category)
                    .OrderBy(c => c.Key)
                    //lets load them all before memory processing
                    .ToList()
                    .Select(c => new ActivityTemplateCategoryDTO
                    {
                        Activities = c.Select(Mapper.Map<ActivityTemplateDTO>),
                        Name = c.Key.ToString()
                    })
                    .ToList();
            }

            return curActivityTemplates;
	    }

        public async Task<List<CrateDTO>> GetCratesByDirection(int activityId, string manifestType, GetCrateDirection direction)
        {
            var httpClient = new HttpClient();

            // TODO: after DO-1214 this must target to "ustream" and "downstream" accordingly.
            var directionSuffix = (direction == GetCrateDirection.Upstream)
                ? "upstream_actions/"
                : "downstream_actions/";

            var url = CloudConfigurationManager.GetSetting("CoreWebServerUrl")
                + "route_nodes/"
                + directionSuffix
                + "?id=" + activityId;

            using (var response = await httpClient.GetAsync(url))
            {
                var content = await response.Content.ReadAsStringAsync();
                var curActions = JsonConvert.DeserializeObject<List<ActionDTO>>(content);

                var curCrates = new List<CrateDTO>();

                foreach (var curAction in curActions)
                {
                    curCrates.AddRange(_crate.GetCratesByManifestType(manifestType, curAction.CrateStorage).ToList());
    }
        
                return curCrates;
            }
        }
    }
}