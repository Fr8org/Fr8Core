namespace terminalAzure.Services
{
    public interface IEvent
    {
        /// <summary>
        /// Processes external event payload from the terminal
        /// </summary>
        void Process(string curExternalEventPayload);
    }
}