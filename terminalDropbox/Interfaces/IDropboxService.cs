using Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace terminalDropbox.Interfaces
{
    public interface IDropboxService
    {
        Task<List<string>> GetFileList(AuthorizationTokenDO authorizationTokenDO);

        string GetFileSharedUrl(AuthorizationTokenDO authorizationTokenDO, string path);
    }
}