namespace TerminalBase.Infrastructure
{
    public class ExplicitDataHubCommunicator : DataHubCommunicatorBase
    {
        protected override string LabelPrefix
        {
            get { return "ExplicitData"; }
        }
    }
}
