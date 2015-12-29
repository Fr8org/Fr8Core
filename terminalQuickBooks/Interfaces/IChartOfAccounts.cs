using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Entities;
using Intuit.Ipp.Data;
using terminalQuickBooks.Infrastructure;

namespace terminalQuickBooks.Interfaces
{
    public interface IChartOfAccounts
    {
        List<Account> GetAccountList(AuthorizationTokenDO authTokenDO);
        List<QuickBooksAccount> GetChartOfAccounts(AuthorizationTokenDO authTokenDO);
    }
}