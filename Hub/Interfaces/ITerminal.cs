using System;
using System.Collections.Generic;
using Data.Entities;
using System.Threading.Tasks;
using Fr8.Infrastructure.Data.DataTransferObjects;

namespace Hub.Interfaces
{
    public interface ITerminal
    {
        IEnumerable<TerminalDO> GetAll();
        IEnumerable<TerminalDO> GetByCurrentUser();
        Task<IList<ActivityTemplateDO>> GetAvailableActivities(string uri);
        TerminalDO GetByKey(Guid terminalId);
        TerminalDO GetByNameAndVersion(string name, string version);
        TerminalDO RegisterOrUpdate(TerminalDO terminalDo, bool isDiscovery);
        Dictionary<string, string> GetRequestHeaders(TerminalDO terminal, string userId);
        Task<TerminalDO> GetByToken(string token);
        Task<List<DocumentationResponseDTO>> GetSolutionDocumentations(string terminalName);
    }
}