using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fr8Data.DataTransferObjects;
using TerminalBase.Infrastructure;
using TerminalBase.Models;

namespace TerminalBase.Interfaces
{
    public interface IActivity
    {
        Task Run(ActivityContext activityContext, ContainerExecutionContext containerExecutionContext);
        Task RunChildActivities(ActivityContext activityContext, ContainerExecutionContext containerExecutionContext);
        Task Configure(ActivityContext activityContext);
        Task Activate(ActivityContext activityContext);
        Task Deactivate(ActivityContext activityContext);
        Task<SolutionPageDTO> GetDocumentation(ActivityContext activityContext, string documentationType);
        void SetHubCommunicator(IHubCommunicator hubCommunicator);
    }
}
