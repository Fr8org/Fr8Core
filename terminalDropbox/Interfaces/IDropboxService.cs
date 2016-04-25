using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace terminalDropbox.Interfaces
{
    public interface IDropboxService
    {
        Task<Dictionary<string, string>> GetFileList(AuthorizationTokenDO authorizationTokenDO);

        Task<string> GetFileSharedUrl(AuthorizationTokenDO authorizationTokenDO, string path);
    }
}