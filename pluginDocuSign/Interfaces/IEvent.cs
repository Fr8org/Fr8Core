using System.Threading.Tasks;

namespace pluginDocuSign.Interfaces
{
    public interface IEvent
    {
        /// <summary>
        /// Processes external event payload from the plugin
        /// </summary>
        Task<object> Process(string curExternalEventPayload);
    }
}