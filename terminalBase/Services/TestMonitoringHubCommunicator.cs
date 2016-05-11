
namespace TerminalBase.Services
{
    public class TestMonitoringHubCommunicator : DataHubCommunicatorBase
    {
        protected override string LabelPrefix
        {
            get { return "HealthMonitor"; }
        }

        public TestMonitoringHubCommunicator(string explicitData) : base(explicitData)
        {
        }
    }
}
