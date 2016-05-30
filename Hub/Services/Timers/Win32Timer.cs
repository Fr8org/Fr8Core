using System;
using System.Threading;
using Hub.Interfaces;

namespace Hub.Services.Timers
{
    public class Win32Timer : ITimer
    {
        private readonly object _sync = new object();
        private Timer _timer;
        
        public void Configure(Action<object> callback, object state, int dueTime, int interval)
        {
            lock (_sync)
            {
                _timer?.Dispose();
                _timer = new Timer(new TimerCallback(callback), state, dueTime, interval);
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
