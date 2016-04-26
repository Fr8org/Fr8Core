using System;
using System.Threading.Tasks;

namespace terminalSlack.Interfaces
{
    public interface ISlackWatcherRepository : IDisposable
    {
        Task Subscribe(string authToken, Guid activityId);

        Task Unsubscribe(Guid activityId);
    }
}
