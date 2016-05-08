using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Entities;
using Data.Interfaces.Manifests;
using Intuit.Ipp.Data;
using terminalQuickBooks.Infrastructure;

namespace terminalQuickBooks.Interfaces
{
    public interface IChartOfAccounts
    {
        ChartOfAccountsCM GetChartOfAccounts(AuthorizationTokenDO authTokenDO, string userId);
    }
}