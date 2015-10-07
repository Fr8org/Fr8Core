using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;

namespace Core.Interfaces
{
	public interface IActivity
	{
		List<ActivityDO> GetUpstreamActivities(IUnitOfWork uow, ActivityDO curActivityDO);
        List<ActivityDO> GetDownstreamActivities(IUnitOfWork uow, ActivityDO curActivityDO);
        Task Process(int curActivityId, ProcessDO curProcessDO);
        IEnumerable<ActivityDO> GetNextActivities(ActivityDO curActivityDO);
        IEnumerable<ActivityTemplateDO> GetAvailableActivities(IUnitOfWork uow, IDockyardAccountDO curAccount);
	}
}