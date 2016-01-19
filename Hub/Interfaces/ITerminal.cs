using System.Collections.Generic;
using Data.Entities;
using System.Threading.Tasks;
using System;

namespace Hub.Interfaces
{
    public interface ITerminal
    {
        IEnumerable<TerminalDO> GetAll();

        Task<IList<ActivityTemplateDO>> GetAvailableActions(string uri);

        TerminalDO GetByKey(int terminalId);
        void RegisterOrUpdate(TerminalDO terminalDo);
        
        Task<TerminalDO> GetTerminalByPublicIdentifier(string terminalId);
        Task<bool> IsUserSubscribedToTerminal(string terminalId, string userId);
    }
}