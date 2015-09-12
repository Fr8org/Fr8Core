using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Core.Interfaces;
using Core.Managers.APIManagers.Transmitters.Plugin;
using Core.Managers.APIManagers.Transmitters.Restful;
using Core.PluginRegistrations;
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
		public List<ActivityDO> GetUpstreamActivities(ActivityDO curActivityDO)
		{
			if (curActivityDO == null)
				throw new ArgumentNullException("curActivityDO");
			List<ActivityDO> result = new List<ActivityDO>();
			using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
			{
				var curActivity = curActivityDO.ParentActivity;
				while (curActivity != null)
				{
					// Get action with lower Ordering
					var lowerAction = curActivity.Activities.OrderBy(x => x.Ordering).FirstOrDefault();
					if (lowerAction != null)
						result.Add(lowerAction);
					result.Add(curActivity);
					// Go to parent ActionListDO
					curActivity = curActivity.ParentActivity;
				}
			}
			return result;
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
			var higherChildren = GetChildren(uow, curActivity).Where(x => x.Ordering > startingOrdering);
			foreach (var higherActivity in higherChildren)
			{
				downstreamList.Add(higherActivity);
				var childActionList = higherActivity as ActionListDO;
				// For any ActionListDO call this method to get downstream
				if (childActionList != null)
					GetDownstreamActivitiesRecursive(uow, childActionList, 0, downstreamList);
			}
		}
		private IEnumerable<ActivityDO> GetChildren(IUnitOfWork uow, ActivityDO currActivity)
		{
			// Get all activities which parent is currActivity and order their by Ordering. The order is important!
			var orderedActivities = uow.ActivityRepository.GetAll()
				.Where(x => x.ParentActivityId == currActivity.Id)
				.OrderBy(z => z.Ordering);
			return orderedActivities;
		}

        public void Process(ActivityDO curActivityDO)
        {
            if (curActivityDO == null)
                throw new ArgumentNullException("ActivityDO is null");

            if (curActivityDO is ActionListDO)
            {
                IActionList _actionList = ObjectFactory.GetInstance<IActionList>();
                _actionList.Process((ActionListDO)curActivityDO);
            }
            else if (curActivityDO is ActionDO)
            {
                IAction _action = ObjectFactory.GetInstance<IAction>();
                _action.Process((ActionDO)curActivityDO);
            }
        }


        public IEnumerable<ActivityDO> GetNextActivities(ActivityDO curActivityDO)
        {
            IEnumerable<ActivityDO> activityLists = new List<ActivityDO>();

            if (curActivityDO == null)
                throw new ArgumentNullException("ActivityDO is null");

            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                activityLists = this.GetChildren(uow, curActivityDO);
            }

            return activityLists;
        }
    }
}