using System.Threading.Tasks;
namespace terminalDocuSign.Interfaces
{
    public interface IEvent
    {
        /// <summary>
        /// Processes external event payload from the Terminal
        /// </summary>
        Task<object> Process(string curExternalEventPayload);
    }
}