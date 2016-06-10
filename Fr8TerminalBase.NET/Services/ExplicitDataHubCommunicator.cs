using Fr8.Infrastructure.Data.Managers;

namespace Fr8.TerminalBase.Services
{
    public class ExplicitDataHubCommunicator : DataHubCommunicatorBase
    {
        protected override string LabelPrefix
        {
            get { return "ExplicitData"; }
        }

        public ExplicitDataHubCommunicator(string explicitData, ICrateManager crateManager)
            : base(explicitData, crateManager)
        {
        }
    }
}
