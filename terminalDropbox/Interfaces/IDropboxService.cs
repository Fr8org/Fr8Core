using System.Collections.Generic;
using System.Threading.Tasks;
using TerminalBase.Models;

namespace terminalDropbox.Interfaces
{
    public interface IDropboxService
    {
        Task<List<string>> GetFileList(AuthorizationToken authorizationToken);

        string GetFileSharedUrl(AuthorizationToken authorizationToken, string path);
    }
}