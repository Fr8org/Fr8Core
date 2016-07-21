using System;
using System.Collections.Generic;
using Hub.Interfaces;

namespace HubTests.Utilization
{
    public class ManualyTriggeredTimerService : ITimer
    {
        private class TimerRegistration
        {
            private readonly Action<object> _callback;
            private readonly WeakReference _state;

            public TimerRegistration(Action<object> callback, object state)
            {
                _callback = callback;
                _state = new WeakReference(state);
            }

            public void Invoke()
            {
                var state = _state.Target;

                _callback?.Invoke(state);
            }
        }

        private readonly List<TimerRegistration> _registrations = new List<TimerRegistration>();

        public void Configure(Action<object> callback, object state, int dueTime, int interval)
        {
            Register(callback, state);
        }

        public object Register(Action<object> callback, object state)
        {
            var registration = new TimerRegistration(callback, state);

            lock (_registrations)
            {
                _registrations.Add(registration);
            }

            return registration;
        }

        public void Unregister(object token)
        {
            lock (_registrations)
            {
                var registration = token as TimerRegistration;

                if (registration != null)
                {
                    _registrations.Remove(registration);
                }
            }
        }

        public void Tick()
        {
            TimerRegistration[] registrations;

            lock (_registrations)
            {
                registrations = _registrations.ToArray();
            }

            foreach (var timerRegistration in registrations)
            {
                timerRegistration.Invoke();
            }
        }

        public void Clear()
        {
            lock (_registrations)
            {
                _registrations.Clear();
            }
        }

        public void Dispose()
        {
        }
    }


    public class ManualyTriggeredTimer : ITimer
    {
        private object _registration;
        private readonly object _sync = new object();
        private readonly ManualyTriggeredTimerService _timerService;

        public ManualyTriggeredTimer(ManualyTriggeredTimerService timerService)
        {
            _timerService = timerService;
        }
        
        public void Configure(Action<object> callback, object state, int dueTime, int interval)
        {
            lock (_sync)
            {
                _timerService.Unregister(_registration);
                _registration = _timerService.Register(callback, state);
            }
        }

        public void Dispose()
        {
            _timerService.Unregister(_registration);
        }
    }
}
