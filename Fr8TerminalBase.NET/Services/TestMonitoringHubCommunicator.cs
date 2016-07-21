using Fr8.Infrastructure.Data.Managers;

namespace Fr8.TerminalBase.Services
{
    public class TestMonitoringHubCommunicator : DataHubCommunicatorBase
    {
        protected override string LabelPrefix
        {
            get { return "HealthMonitor"; }
        }

        public TestMonitoringHubCommunicator(string explicitData, ICrateManager crateManager)
            : base(explicitData, crateManager)
        {
        }
    }
}
