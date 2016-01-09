using System.Collections.Generic;
using Data.Entities;
using System.Threading.Tasks;
using System;

namespace Hub.Interfaces
{
    public interface ITerminal
    {
        IEnumerable<TerminalDO> GetAll();

        /// <summary>
        /// Parses the required terminal service URL for the given action by Terminal Name and its version
        /// </summary>
        /// <param name="curTerminalName">Name of the required terminal</param>
        /// <param name="curTerminalVersion">Version of the required terminal</param>
        /// <param name="curActionName">Required action</param>
        /// <returns>Parsed URl to the terminal for its action</returns>
        string ParseTerminalUrlFor(string curTerminalName, string curTerminalVersion, string curActionName);

        Task<IList<string>> RegisterTerminals(string uri);
        Task<IList<ActivityTemplateDO>> GetAvailableActions(string uri);

        Task<TerminalDO> GetTerminalById(Guid Id);
    }
}