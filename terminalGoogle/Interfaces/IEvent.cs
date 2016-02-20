using Data.Crates;
using Data.Interfaces.DataTransferObjects;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
namespace terminalGoogle.Infrastructure
{
    public interface IEvent
    {
        Task<Crate> Process(string externalEventPayload);
    }
}
