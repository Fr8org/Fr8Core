using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Data.Entities;
using terminalQuickBooks.Infrastructure;
using terminalQuickBooks.Interfaces;

namespace terminalQuickBooks.Services
{
    public class ChartOfAccounts: IChartOfAccounts
    {
        /// <summary>
        /// Obtains list of accounts from Quick
        /// </summary>
        /// <param name="authTokenDO"></param>
        /// <returns>List of Accounts of Intuit type</returns>
        public List<Intuit.Ipp.Data.Account> GetAccountList(AuthorizationTokenDO authTokenDO)
        {
            var _qbConnectivity = new Connectivity();
            var curDataService = _qbConnectivity.GetDataService(authTokenDO);
            var curAccountList = curDataService.FindAll(new Intuit.Ipp.Data.Account()).ToList();
            return curAccountList;
        }
        /// <summary>
        /// Returns a list of QuickBooksAccounts, simplified version of Intuit Account class
        /// </summary>
        /// <param name="authTokenDO"></param>
        /// <returns></returns>
        public List<QuickBooksAccount> GetChartOfAccounts(AuthorizationTokenDO authTokenDO)
        {
            var listOfAccounts = GetAccountList(authTokenDO);
            if (listOfAccounts.Count == 0)
            {
                throw new Exception("No Accounts found in the QuickBooks account");
            }
            var listOfQuickBooksAccounts = new List<QuickBooksAccount>();
            foreach (var curAccount in listOfAccounts)
            {
                var curQuickBooksAccount = new QuickBooksAccount();
                curQuickBooksAccount.Id = curAccount.Id;
                curQuickBooksAccount.Name = curAccount.Name;
                listOfQuickBooksAccounts.Add(curQuickBooksAccount);
            }
            return listOfQuickBooksAccounts;
        }
    }
}