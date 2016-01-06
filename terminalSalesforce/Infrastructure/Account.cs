using System.Net.Http;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Hub.Managers;
using StructureMap;
using terminalSalesforce.Services;

namespace terminalSalesforce.Infrastructure
{
    public class Account
    {
        ForceClient client;     
        private ICrateManager _crateManager;

        public Account()
        {
            _crateManager = ObjectFactory.GetInstance<ICrateManager>();
        }

        public async Task CreateAccount(ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            
            string instanceUrl, apiVersion;
            ParseAuthToken(authTokenDO.AdditionalAttributes, out instanceUrl, out apiVersion);
            client = new ForceClient(instanceUrl, authTokenDO.Token, apiVersion);
            AccountDTO account = new AccountDTO();
            var curFieldList = _crateManager.GetStorage(actionDO).CrateContentsOfType<StandardConfigurationControlsCM>().First();
            account.Name = curFieldList.Controls.First(x => x.Name == "accountName").Value;           
            account.AccountNumber = curFieldList.Controls.First(x => x.Name == "accountNumber").Value;
            account.Phone = curFieldList.Controls.First(x => x.Name == "phone").Value;
            if (!String.IsNullOrEmpty(account.Name))
            {
                var accountId = await client.CreateAsync("Account", account);
            }
        }

        public async Task<object> GetAccountFields(ActionDO actionDO, AuthorizationTokenDO authTokenDO)
        {
            string instanceUrl, apiVersion;
            ParseAuthToken(authTokenDO.AdditionalAttributes, out instanceUrl, out apiVersion);

            //HttpClient c = new HttpClient();
            client = new ForceClient(instanceUrl, authTokenDO.Token, apiVersion);//, c);
            var accountFields = await client.DescribeAsync<object>("Account");
            return Task.FromResult(accountFields);
        }

        public void ParseAuthToken(string authonTokenAdditionalValues, out string instanceUrl, out string apiVersion)
        {
            int startIndexOfInstanceUrl = authonTokenAdditionalValues.IndexOf("instance_url");
            int startIndexOfApiVersion = authonTokenAdditionalValues.IndexOf("api_version");
            instanceUrl = authonTokenAdditionalValues.Substring(startIndexOfInstanceUrl, (startIndexOfApiVersion - 1 - startIndexOfInstanceUrl));
            apiVersion = authonTokenAdditionalValues.Substring(startIndexOfApiVersion, authonTokenAdditionalValues.Length - startIndexOfApiVersion);
            instanceUrl = instanceUrl.Replace("instance_url=", "");
            apiVersion = apiVersion.Replace("api_version=", "");
        }
       
    }
}