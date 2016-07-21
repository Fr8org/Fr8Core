namespace TerminalBase.Services
{
    public class ExplicitDataHubCommunicator : DataHubCommunicatorBase
    {
        protected override string LabelPrefix
        {
            get { return "ExplicitData"; }
        }

        public ExplicitDataHubCommunicator(string explicitData) : base(explicitData)
        {
        }
    }
}
