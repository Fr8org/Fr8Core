using System.Collections.Generic;
using System.Threading.Tasks;
using Fr8.TerminalBase.Models;
using Fr8.Infrastructure.Data.Manifests;

namespace terminalDropbox.Interfaces
{
    public interface IDropboxService
    {
        Task<List<string>> GetFileList(AuthorizationToken authorizationToken);
        Task<StandardFileDescriptionCM> GetFile(AuthorizationToken authorizationToken, string path);
        string GetFileSharedUrl(AuthorizationToken authorizationToken, string path);
    }
}