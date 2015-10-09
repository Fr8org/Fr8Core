using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Interfaces;
using Data.Entities;
using Data.Interfaces;
using StructureMap;

namespace Core.Services
{
	public class Activity : IActivity
	{
        /**********************************************************************************/
        // Functions
        /**********************************************************************************/

		public Activity()
		{
		}

        /**********************************************************************************/
        //This builds a list of an activity and all of its descendants, over multiple levels
	    public List<ActivityDO> GetActivityTree(IUnitOfWork uow, ActivityDO curActivity)
	    {
	        var curList = new List<ActivityDO>();
	        curList.Add(curActivity);
            var childActivities = GetChildren(uow, curActivity);
	        foreach (var child in childActivities)
	        {
	           curList.AddRange(GetActivityTree(uow, child));
	        }
	        return curList;
	    }

        /**********************************************************************************/

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

	    /**********************************************************************************/

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

        /**********************************************************************************/

        public void Delete (IUnitOfWork uow, ActivityDO activity)
        {
            var activities = new List<ActivityDO>();

            TraverseActivity(activity, activities.Add);

            activities.ForEach(x =>
            {
                // TODO: it is not very smart solution. Activity service should not knon about anything except Activities
                if (x is ProcessNodeTemplateDO)
                {
                    foreach (var criteria in uow.CriteriaRepository.GetQuery().Where(y => y.ProcessNodeTemplateId == x.Id).ToArray())
                    {
                        uow.CriteriaRepository.Remove(criteria);
                    }
                }

                uow.ActivityRepository.Remove(x);
            });
        }

        /**********************************************************************************/

        private static void TraverseActivity(ActivityDO parent, Action<ActivityDO> visitAction)
        {
            visitAction(parent);
            foreach (ActivityDO child in parent.Activities)
                TraverseActivity(child, visitAction);
        }
        
        /**********************************************************************************/

	    private IEnumerable<ActivityDO> GetChildren(IUnitOfWork uow, ActivityDO currActivity)
		{
                // Get all activities which parent is currActivity and order their by Ordering. The order is important!
                var orderedActivities = uow.ActivityRepository.GetAll()
                .Where(x => x.ParentActivityId == currActivity.Id)
                .OrderBy(z => z.Ordering);
                return orderedActivities;
            }
		   
	    /**********************************************************************************/

        public async Task Process(int curActivityId, ProcessDO processDO)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curProcessDO = uow.ProcessRepository.GetByKey(processDO.Id);
                var curActivityDO = uow.ActivityRepository.GetByKey(curActivityId);

                if (curActivityDO == null)
                {
                    throw new ArgumentException("Cannot find Activity with the supplied curActivityId");
                }

                if (curActivityDO is ActionDO)
                {
                    IAction _action = ObjectFactory.GetInstance<IAction>();
                    await _action.PrepareToExecute((ActionDO) curActivityDO, curProcessDO, uow);
            }
        }
            }

        /**********************************************************************************/

        public IEnumerable<ActivityTemplateDO> GetAvailableActivities(IUnitOfWork uow, IDockyardAccountDO curAccount)
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

        /**********************************************************************************/
    }
}