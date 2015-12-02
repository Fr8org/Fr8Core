using Data.Interfaces.DataTransferObjects;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
namespace terminalGoogle.Infrastructure
{
    public interface IEvent
    {
        Task<object> Process(string externalEventPayload);
    }
}
