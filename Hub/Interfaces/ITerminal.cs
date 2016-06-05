using System.Collections.Generic;
using Data.Entities;
using System.Threading.Tasks;
using System;
using Fr8Data.DataTransferObjects;

namespace Hub.Interfaces
{
    public interface ITerminal
    {
        IEnumerable<TerminalDO> GetAll();

        Task<IList<ActivityTemplateDO>> GetAvailableActivities(string uri);

        TerminalDO GetByKey(int terminalId);
        TerminalDO GetByNameAndVersion(string name, string version);
        TerminalDO RegisterOrUpdate(TerminalDO terminalDo);

        Task<TerminalDO> GetTerminalByPublicIdentifier(string terminalId);
        Task<bool> IsUserSubscribedToTerminal(string terminalId, string userId);
        Task<List<DocumentationResponseDTO>> GetSolutionDocumentations(string terminalName);
    }
}