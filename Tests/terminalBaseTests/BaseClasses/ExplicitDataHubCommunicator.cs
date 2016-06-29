using Fr8.Infrastructure.Data.Managers;
using Fr8.TerminalBase.Services;

namespace terminaBaselTests.BaseClasses
{
    public class ExplicitDataHubCommunicator : DataHubCommunicatorBase
    {
        protected override string LabelPrefix => "ExplicitData";

        public ExplicitDataHubCommunicator(string explicitData, ICrateManager crateManager)
            : base(explicitData, crateManager)
        {
        }
    }
}
