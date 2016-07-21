using System;

namespace Hub.Interfaces
{
    public interface ITimer : IDisposable
    {
        void Configure(Action<object> callback, object state, int dueTime, int interval);
    }
}
