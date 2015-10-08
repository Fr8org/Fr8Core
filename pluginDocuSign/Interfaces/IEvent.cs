using Data.Interfaces.DataTransferObjects;
namespace pluginDocuSign.Interfaces
{
    public interface IEvent
    {
        /// <summary>
        /// Processes external event payload from the plugin
        /// </summary>
        CrateDTO ProcessEvent(string curExternalEventPayload);
    }
}