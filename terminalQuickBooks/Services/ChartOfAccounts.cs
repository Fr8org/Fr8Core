using System;
using System.Collections.Generic;
using System.Linq;
using Fr8.Infrastructure.Data.Manifests;
using Fr8.TerminalBase.Interfaces;
using Fr8.TerminalBase.Models;
using StructureMap;
using terminalQuickBooks.Interfaces;

namespace terminalQuickBooks.Services
{
    public class ChartOfAccounts: IChartOfAccounts
    {

        /// <summary>
        /// Obtains list of accounts from Quick
        /// </summary>
        /// <param name="authTokenDO"></param>
        /// <param name="userId"/>
        /// <returns>List of Accounts of Intuit type</returns>
        public List<Intuit.Ipp.Data.Account> GetAccountList(AuthorizationToken authToken,  IHubCommunicator hubCommunicator, IServiceWorker serviceWorker)
        {
            var curDataService = serviceWorker.GetDataService(authToken,  hubCommunicator);
            var curAccountList = curDataService.FindAll(new Intuit.Ipp.Data.Account()).ToList();
            return curAccountList;
        }
        /// <summary>
        /// Returns a list of QuickBooksAccounts, simplified version of Intuit Account class
        /// </summary>
        /// <param name="authTokenDO"></param>
        /// <param name="userId"/>
        /// <returns></returns>
        public ChartOfAccountsCM GetChartOfAccounts(AuthorizationToken authToken,  IHubCommunicator hubCommunicator, IServiceWorker serviceWorker)
        {
            var listOfAccounts = GetAccountList(authToken, hubCommunicator, serviceWorker);
            if (listOfAccounts.Count == 0)
            {
                throw new Exception("No Accounts found in the QuickBooks account");
            }
            var listOfQBAccounts = new List<AccountDTO>();
            foreach (var curAccount in listOfAccounts)
            {
                var curQuickBooksAccount = new AccountDTO();
                curQuickBooksAccount.Id = curAccount.Id;
                curQuickBooksAccount.Name = curAccount.Name;
                listOfQBAccounts.Add(curQuickBooksAccount);
            }
            return new ChartOfAccountsCM()
            {
                Accounts = listOfQBAccounts
            };
        }
    }
}