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

        public List<ActivityDO> GetUpstreamActivities(IUnitOfWork uow, ActivityDO curActivityDO)
		{
			if (curActivityDO == null)
				throw new ArgumentNullException("curActivityDO");
            if (curActivityDO.ParentActivityId == null)
                return new List<ActivityDO>();

			List<ActivityDO> upstreamActivities = new List<ActivityDO>();

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


	    public List<ActivityDO> GetDownstreamActivities(IUnitOfWork uow, ActivityDO curActivity)
	    {
	        if (curActivity == null)
	            throw new ArgumentNullException("curActivity");
	        if (curActivity.ParentActivityId == null)
	            return new List<ActivityDO>();

	        List<ActivityDO> downstreamList = new List<ActivityDO>();

	        //start by getting the parent of the current action
	        var parentActivity = uow.ActivityRepository.GetByKey(curActivity.ParentActivityId);

	        // find all sibling actions that have a higher Ordering. These are the ones that are "below" or downstream of this action in the list
	        var downstreamSiblings = parentActivity.Activities.Where(a => a.Ordering > curActivity.Ordering);

	        //for each such sibling action, we want to add it to the list
	        //but some of those activities may be actionlists with childactivities of their own
	        //in that case we need to recurse
	        foreach (var downstreamSibling in downstreamSiblings)
	        {
	            //1) first add the downstream siblings and their descendants
	            downstreamList.AddRange(GetActivityTree(downstreamSibling));
	        }

	        //now we need to recurse up to the parent of the current activity, and repeat until we reach the root of the tree
	        if (parentActivity != null)
	        {
	            //find the downstream siblings of the parent activity and add them and their descendants

	            downstreamList.AddRange(GetDownstreamActivities(uow, parentActivity));
	        }

	        return downstreamList;
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

        public async Task Process(int curActivityId, ProcessDO processDO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curProcessDO = uow.ProcessRepository.GetByKey(processDO.Id);

                var curActivityDO = uow.ActivityRepository.GetByKey(curActivityId);

                if (curActivityDO == null)
                    throw new ArgumentException("Cannot find Activity with the supplied curActivityId");

                if (curActivityDO is ActionListDO)
                {
                    IActionList _actionList = ObjectFactory.GetInstance<IActionList>();
                    _actionList.Process((ActionListDO)curActivityDO, curProcessDO, uow);
                }
                else if (curActivityDO is ActionDO)
                {
                    IAction _action = ObjectFactory.GetInstance<IAction>();
                    await _action.PrepareToExecute((ActionDO)curActivityDO, curProcessDO, uow);
                }
            }
        }


        public IEnumerable<ActivityDO> GetNextActivities(ActivityDO curActivityDO)
        {
            IEnumerable<ActivityDO> activityLists = new List<ActivityDO>();

            if (curActivityDO == null)
                throw new ArgumentNullException("ActivityDO is null");

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                // commented out by yakov.gnusin in scope of DO-1153.
                // activityLists = this.GetChildren(curActivityDO);

                // workaround for now.
                activityLists = uow.ActivityRepository.GetQuery()
                    .Where(x => x.ParentActivityId == curActivityDO.ParentActivityId)
                    .Where(x => x.Ordering > curActivityDO.Ordering)
                    .OrderBy(x => x.Ordering)
                    .ToList();
            }

            return activityLists;
        }

        public IEnumerable<ActivityTemplateDO> GetAvailableActivities(IDockyardAccountDO curAccount)
        {
            List<ActivityTemplateDO> curActivityTemplates;

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                curActivityTemplates = uow.ActivityTemplateRepository.GetAll().OrderBy(t => t.Category).ToList();
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

	    public IEnumerable<ActivityTemplateCategoryDTO> GetAvailableActivitiyGroups(IDockyardAccountDO curAccount)
	    {
            //TODO make this function use curAccount !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
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
	}
}