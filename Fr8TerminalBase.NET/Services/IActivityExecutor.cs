using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Fr8.TerminalBase.Services
{
    public interface IActivityExecutor
    {
        Task<object> HandleFr8Request(string curActionPath, IEnumerable<KeyValuePair<string, string>> parameters, Fr8DataDTO curDataDTO);
    }
}