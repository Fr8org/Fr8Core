using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Entities;
using Intuit.Ipp.Data;

namespace terminalQuickBooks.Interfaces
{
    public interface IAccount
    {
        List<Account> GetAccountList(AuthorizationTokenDO authTokenDO);
    }
}