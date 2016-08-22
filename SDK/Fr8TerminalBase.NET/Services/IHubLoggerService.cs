using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.TerminalBase.Services
{
    public interface IHubLoggerService
    {
        Task Log(LoggingDataCM data);
    }
}
