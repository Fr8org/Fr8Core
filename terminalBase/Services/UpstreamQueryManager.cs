using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fr8Data.Crates;
using Fr8Data.DataTransferObjects;
using Fr8Data.Manifests;
using Fr8Data.States;
using TerminalBase.Infrastructure;
using TerminalBase.Models;

namespace TerminalBase.Services
{
    public class UpstreamQueryManager
    {
        private readonly ActivityContext _activityContext;
        private readonly IHubCommunicator _hubCommunicator;
        private Guid ActivityId => _activityContext.ActivityPayload.Id;

        public UpstreamQueryManager(ActivityContext activityContext, IHubCommunicator hubCommunicator)
        {
            _activityContext = activityContext;
            _hubCommunicator = hubCommunicator;
        }

       
    }
}
