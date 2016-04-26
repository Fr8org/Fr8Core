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
        Task<List<string>> GetFileList(AuthorizationTokenDO authorizationTokenDO);

        string GetFileSharedUrl(AuthorizationTokenDO authorizationTokenDO, string path);
    }
}