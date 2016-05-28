using System;
using Hangfire;
using Hub.Interfaces;
using Hangfire.States;
using Hangfire.Common;

namespace Hub.Managers
{
    public class HangfireJobDispatcher : IJobDispatcher
    {
        public void Enqueue(System.Linq.Expressions.Expression<Action> job)
        {
            BackgroundJob.Enqueue(job);
        }
    }
}
