using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Interfaces;
using Core.Managers.APIManagers.Transmitters.Plugin;
using Core.Managers.APIManagers.Transmitters.Restful;

using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Interfaces.DataTransferObjects;
using Data.States;
using Data.Wrappers;
using StructureMap;
using Utilities.Serializers.Json;
using System.Data.Entity;

namespace Core.Services
{
	public class Activity : IActivity
	{
		public Activity()
		{
		}


        //This builds a list of an activity and all of its descendants, over multiple levels
	    public List<ActivityDO> GetActivityTree(ActivityDO curActivity)
	    {
	        var curList = new List<ActivityDO>();
	        curList.Add(curActivity);
	        var childActivities = GetChildren(curActivity);
	        foreach (var child in childActivities)
	        {
	           curList.AddRange(GetActivityTree(child));
	        }
	        return curList;
	    }

        public List<ActivityDO> GetUpstreamActivities(ActivityDO curActivityDO)
		{
			if (curActivityDO == null)
				throw new ArgumentNullException("curActivityDO");
            if (curActivityDO.ParentActivityId == null)
                return new List<ActivityDO>();

			List<ActivityDO> upstreamActivities = new List<ActivityDO>();

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
                //start by getting the parent of the current action
                var parentActivity = uow.ActivityRepository.GetByKey(curActivityDO.ParentActivityId);


				// find all sibling actions that have a lower Ordering. These are the ones that are "above" this action in the list
				var upstreamSiblings =
				    parentActivity.Activities.Where(a => a.Ordering < curActivityDO.Ordering);
                        
                //for each such sibling action, we want to add it to the list
                //but some of those activities may be actionlists with childactivities of their own
                //in that case we need to recurse
				foreach (var upstreamSibling in upstreamSiblings)
				{
                    //1) first add the upstream siblings
				    upstreamActivities.AddRange(GetActivityTree(upstreamSibling));
				}

                //now we need to recurse up to the parent of the current parent, and repeat until we reach the root of the tree
				if (parentActivity != null)
				{
                    //2) then add the parent activity...
                    upstreamActivities.Add(parentActivity); 
                    //3) then add the parent's upstream activities
				    upstreamActivities.AddRange(GetUpstreamActivities(parentActivity));
				}
				else return upstreamActivities;
					    
			}
            return upstreamActivities; //should never actually get here, but the compiler insists
		}


    public List<ActivityDO> GetDownstreamActivities(ActivityDO curActivityDO)
		{
			if (curActivityDO == null)
				throw new ArgumentNullException("curActivityDO");
			List<ActivityDO> downstreamList = new List<ActivityDO>();
			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				ActivityDO curActivity = curActivityDO;
				int startingOrdering = curActivity.Ordering;
				while (curActivity != null)
				{
					GetDownstreamActivitiesRecursive(uow, curActivity, startingOrdering, downstreamList);
					//work up the parent ActionList chain to get activities that are downstream of the path.
					startingOrdering = curActivity.Ordering;
					curActivity = curActivity.ParentActivity;
				}
			}
			return downstreamList;
		}
		private void GetDownstreamActivitiesRecursive(IUnitOfWork uow, ActivityDO curActivity, int startingOrdering, List<ActivityDO> downstreamList)
		{
			// Get the higher ordered (downstream) activities for the current ActionList
			var higherChildren = GetChildren(curActivity).Where(x => x.Ordering > startingOrdering);
			foreach (var higherActivity in higherChildren)
			{
				downstreamList.Add(higherActivity);
				var childActionList = higherActivity as ActionListDO;
				// For any ActionListDO call this method to get downstream
				if (childActionList != null)
					GetDownstreamActivitiesRecursive(uow, childActionList, 0, downstreamList);
			}
		}
		private IEnumerable<ActivityDO> GetChildren(ActivityDO currActivity)
		{
		    using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
		    {
                // Get all activities which parent is currActivity and order their by Ordering. The order is important!
                var orderedActivities = uow.ActivityRepository.GetAll()
                .Where(x => x.ParentActivityId == currActivity.Id)
                .OrderBy(z => z.Ordering);
                return orderedActivities;
            }
		   
		}

        public void Process(ActivityDO curActivityDO, ProcessDO curProcessDO)
        {
            if (curActivityDO == null)
                throw new ArgumentNullException("ActivityDO is null");

            if (curActivityDO is ActionListDO)
            {
                IActionList _actionList = ObjectFactory.GetInstance<IActionList>();
                _actionList.Process((ActionListDO)curActivityDO, curProcessDO);
            }
            else if (curActivityDO is ActionDO)
            {
                IAction _action = ObjectFactory.GetInstance<IAction>();
                _action.Process((ActionDO)curActivityDO, curProcessDO);
            }
        }


        public IEnumerable<ActivityDO> GetNextActivities(ActivityDO curActivityDO)
        {
            IEnumerable<ActivityDO> activityLists = new List<ActivityDO>();

            if (curActivityDO == null)
                throw new ArgumentNullException("ActivityDO is null");

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                activityLists = this.GetChildren(curActivityDO);
            }

            return activityLists;
        }

        public IEnumerable<ActivityTemplateDO> GetAvailableActivities(IDockyardAccountDO curAccount)
        {
            List<ActivityTemplateDO> curActivityTemplates;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curActivityTemplates = uow.ActivityTemplateRepository.GetAll().ToList();
            }

            //we're currently bypassing the subscription logic until we need it
            //we're bypassing the pluginregistration logic here because it's going away in V2

            //var plugins = _subscription.GetAuthorizedPlugins(curAccount);
            //var plugins = _plugin.GetAll();
            // var curActionTemplates = plugins
            //    .SelectMany(p => p.AvailableActions)
            //    .OrderBy(s => s.ActionType);

            return curActivityTemplates;
        }
    }
}