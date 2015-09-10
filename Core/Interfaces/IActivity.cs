using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;

namespace Core.Interfaces
{
	public interface IActivity
	{
		List<ActivityDO> GetUpstreamActivities(ActivityDO curActivityDO);
		List<ActivityDO> GetDownstreamActivities(ActivityDO curActivityDO);
        void Process(ActivityDO curActivityDO);
	}
}