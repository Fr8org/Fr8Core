using System.Collections.Generic;
using Data.Entities;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Hub.Interfaces
{
    public interface ITerminal
    {
        IEnumerable<TerminalDO> GetAll();

        Task<IList<ActivityTemplateDO>> GetAvailableActivities(string uri);

        TerminalDO GetByKey(int terminalId);
        TerminalDO GetByNameAndVersion(string name, string version);
        TerminalDO RegisterOrUpdate(TerminalDO terminalDo);
        Dictionary<string, string> GetRequestHeaders(TerminalDO terminal);
      
        Task<TerminalDO> GetTerminalByPublicIdentifier(string terminalId);
        Task<TerminalDO> GetByToken(string token);
        Task<bool> IsUserSubscribedToTerminal(string terminalId, string userId);
        Task<List<DocumentationResponseDTO>> GetSolutionDocumentations(string terminalName);
    }
}