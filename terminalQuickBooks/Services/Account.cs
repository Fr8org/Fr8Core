using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using terminalQuickBooks.Interfaces;

namespace terminalQuickBooks.Services
{
    public class Account: IAccount
    {
        public List<Intuit.Ipp.Data.Account> GetAccountList(Data.Entities.AuthorizationTokenDO authTokenDO)
        {
            var _quickBooksIntegration = new QuickBooksIntegration();
            var curDataService = _quickBooksIntegration.GetDataService(authTokenDO);
            var curAccountList = curDataService.FindAll(new Intuit.Ipp.Data.Account()).ToList();
            return curAccountList;
        }
    }
}