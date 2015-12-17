using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using terminalQuickBooks.Interfaces;

namespace terminalQuickBooks.Services
{
    public class Account: IAccount
    {
        /// <summary>
        /// Obtains list of accounts from Quick
        /// </summary>
        /// <param name="authTokenDO"></param>
        /// <returns></returns>
        public List<Intuit.Ipp.Data.Account> GetAccountList(Data.Entities.AuthorizationTokenDO authTokenDO)
        {
            var _quickBooksIntegration = new QuickBooksIntegration();
            var curDataService = _quickBooksIntegration.GetDataService(authTokenDO);
            var curAccountList = curDataService.FindAll(new Intuit.Ipp.Data.Account()).ToList();
            return curAccountList;
        }
    }
}