using System.Net.Http;
using Data.Entities;
using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Data.States;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public async Task<IList<FieldDTO>> GetFields(AuthorizationTokenDO curAuthTokenDO)
        {
            string instanceUrl, apiVersion;
            ParseAuthToken(curAuthTokenDO.AdditionalAttributes, out instanceUrl, out apiVersion);

            client = new ForceClient(instanceUrl, curAuthTokenDO.Token, apiVersion);
            var fieldsQueryResponse = (JObject)await client.DescribeAsync<object>("Account");

            var objectFields = new List<FieldDTO>();

            JToken accountFields;
            if (fieldsQueryResponse.TryGetValue("fields", out accountFields) && accountFields is JArray)
            {
                objectFields.AddRange(
                    accountFields.Select(a => new FieldDTO(a.Value<string>("name"), a.Value<string>("label"))));
            }

            return objectFields;
        }

        public async Task<IList<AccountDTO>> GetByQuery(AuthorizationTokenDO curAuthTokenDO, string conditionQuery)
        {
            string instanceUrl, apiVersion;
            ParseAuthToken(curAuthTokenDO.AdditionalAttributes, out instanceUrl, out apiVersion);
            client = new ForceClient(instanceUrl, curAuthTokenDO.Token, apiVersion);

            //if condition is empty, Query for all Accounts
            //else query Accounts with the given query
            if (string.IsNullOrEmpty(conditionQuery))
            {
                conditionQuery = "select Name, AccountNumber, Phone from Account";
            }
            else
            {
                conditionQuery = "select Name, AccountNumber, Phone from Account where " + conditionQuery;
            }

            var response = await client.QueryAsync<object>(conditionQuery);

            var resultAccounts = new List<AccountDTO>();

            if (response.Records.Count > 0)
            {
                resultAccounts.AddRange(
                    response.Records.Select(record => ((JObject) record).ToObject<AccountDTO>()));
            }

            return resultAccounts;
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