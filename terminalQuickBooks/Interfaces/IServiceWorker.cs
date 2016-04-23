using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Intuit.Ipp.Core;
using Intuit.Ipp.DataService;

namespace terminalQuickBooks.Interfaces
{
    public interface IServiceWorker
    {
        DataService GetDataService(AuthorizationTokenDO authTokenDO);
        ServiceContext CreateServiceContext(string oauthToken);
    }
}