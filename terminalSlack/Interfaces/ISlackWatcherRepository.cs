using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace terminalSlack.Interfaces
{
    public interface ISlackWatcherRepository
    {
        Task<ISlackWatcher> GetOrSubscribe(Guid activityId);

        Task Unsubscribe(Guid activityId);
    }
}
