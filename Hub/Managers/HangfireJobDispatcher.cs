using Data.Crates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hub.Interfaces;

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
