using Hangfire.Common;
using Hangfire.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Configuration
{
    public class MoveToTheHubQueueAttribute : JobFilterAttribute, IElectStateFilter
    {
        public void OnStateElection(ElectStateContext context)
        {
            var state = context.CandidateState as FailedState;
            if (state != null && context.BackgroundJob.Job != null && context.BackgroundJob.Job.Type.FullName.StartsWith("HubWeb"))
            {
                context.CandidateState = new ScheduledState(new TimeSpan(0, 0, 0, 5));
            }

        }
    }
}
