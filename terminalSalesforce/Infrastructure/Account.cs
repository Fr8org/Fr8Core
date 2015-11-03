using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using terminalSalesforce.Services;

namespace terminalSalesforce.Infrastructure
{
    public class Account
    {
        ForceClient client;     

        public async Task CreateAccount(ActionDTO currentActionDTO)
        {
            
            string instanceUrl, apiVersion;
            ParseAuthToken(currentActionDTO.AuthToken.AdditionalAttributes, out instanceUrl, out apiVersion);
            client = new ForceClient(instanceUrl, currentActionDTO.AuthToken.Token, apiVersion);
            AccountDTO account = new AccountDTO();
            var curFieldList =
               JsonConvert.DeserializeObject<StandardConfigurationControlsCM>(currentActionDTO.CrateStorage.CrateDTO.First(field => field.Contents.Contains("accountName")).Contents);
            account.Name = curFieldList.Controls.First(x => x.Name == "accountName").Value;           
            account.AccountNumber = curFieldList.Controls.First(x => x.Name == "accountNumber").Value;
            account.Phone = curFieldList.Controls.First(x => x.Name == "phone").Value;
            if (!String.IsNullOrEmpty(account.Name))
            {
                var accountId = await client.CreateAsync("Account", account);
            }
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