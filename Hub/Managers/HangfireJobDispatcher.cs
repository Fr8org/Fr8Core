using Data.Crates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            //put Hubs job in "hub" queue to avoid processing of terminalDocuSign jobs
            var client = new BackgroundJobClient();
            var state = new EnqueuedState("hub");
            var hangfire_job = Job.FromExpression(job);
            client.Create(job, state);
        }
    }
}
