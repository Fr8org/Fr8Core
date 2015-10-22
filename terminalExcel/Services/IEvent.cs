namespace terminalExcel.Services
{
    public interface IEvent
    {
        /// <summary>
        /// Processes external event payload from the plugin
        /// </summary>
        void Process(string curExternalEventPayload);
    }
}