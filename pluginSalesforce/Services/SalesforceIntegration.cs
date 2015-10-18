using Data.Interfaces.DataTransferObjects;
using pluginSalesforce.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using StructureMap;
using Utilities.Logging;
using Salesforce.Force;
using System.Threading.Tasks;
using Salesforce.Common;
using Microsoft.WindowsAzure;
using System.Net.Http;
using Newtonsoft.Json;
using Data.Interfaces.ManifestSchemas;
using Salesforce.Common.Models;
using Data.Interfaces;

namespace pluginSalesforce.Services
{
    public class SalesforceIntegration : ISalesforceIntegration
    {
        ForceClient client;
        string salesforceConsumerKey = string.Empty;
        string salesforceAuthUrl = string.Empty;
        string SalesforceAuthCallbackURLDomain = string.Empty;
        string salesforceConsumerSecret = string.Empty;
        string tokenRequestEndpointUrl = string.Empty;

        public SalesforceIntegration()
        {
            salesforceConsumerKey = CloudConfigurationManager.GetSetting("SalesforceConsumerKey");
            salesforceAuthUrl = CloudConfigurationManager.GetSetting("SalesforceAuthURL");
            SalesforceAuthCallbackURLDomain = CloudConfigurationManager.GetSetting("SalesforceAuthCallbackURLDomain");
            salesforceConsumerSecret = CloudConfigurationManager.GetSetting("SalesforceConsumerSecret");
            tokenRequestEndpointUrl = CloudConfigurationManager.GetSetting("tokenRequestEndpointUrl");
        }

        public bool CreateLead(ActionDTO currentDTO)
        {
            bool createFlag = true;
            try
            {
                var actionDTO = Task.Run(() => RefreshAccessToken(currentDTO)).Result;
                currentDTO = actionDTO;
                var createtask = CreateLeadInSalesforce(currentDTO);
            }
            catch (Exception ex)
            {
                createFlag = false;
                Logger.GetLogger().Error(ex);
                throw;
            }
            return createFlag;
        }

        public async Task<object> GetAuthToken(string code)
        {
            var auth = new AuthenticationClient();
            code = code.Replace("%3D", "=");
            string redirectUrl = SalesforceAuthCallbackURLDomain + "/ExternalAuth/AuthSuccess?dockyard_plugin=pluginSalesforce&version=1";
            await auth.WebServerAsync(salesforceConsumerKey, salesforceConsumerSecret, redirectUrl, code, tokenRequestEndpointUrl);
            return auth;
        }


        private async Task<ActionDTO> RefreshAccessToken(ActionDTO currentActionDTO)
        {
            var auth = new AuthenticationClient();
            await auth.TokenRefreshAsync(salesforceConsumerKey, currentActionDTO.AuthToken.RefreshToken);
            currentActionDTO.AuthToken.Token = auth.AccessToken;
            currentActionDTO.AuthToken.ExternalInstanceURL = auth.InstanceUrl;
            currentActionDTO.AuthToken.ExternalApiVersion = auth.ApiVersion;
            return currentActionDTO;
        }

        private async Task CreateLeadInSalesforce(ActionDTO currentActionDTO)
        {
            client = new ForceClient(currentActionDTO.AuthToken.ExternalInstanceURL, currentActionDTO.AuthToken.Token, currentActionDTO.AuthToken.ExternalApiVersion);
            LeadDTO lead = new LeadDTO();
            var curConnectionStringFieldList =
               JsonConvert.DeserializeObject<StandardConfigurationControlsCM>(currentActionDTO.CrateStorage.CrateDTO.First(field => field.Contents.Contains("firstName")).Contents);
            lead.FirstName = curConnectionStringFieldList.Controls[0].Value;
            lead.LastName = curConnectionStringFieldList.Controls[1].Value;
            lead.Company = curConnectionStringFieldList.Controls[2].Value;
            if (!String.IsNullOrEmpty(lead.LastName) && !String.IsNullOrEmpty(lead.Company))
            {
                var newLeadId = await client.CreateAsync("Lead", lead);
            }
        }

        public string CreateAuthUrl(string exteranalStateValue)
        {
            string redirectUrl = SalesforceAuthCallbackURLDomain + "/ExternalAuth/AuthSuccess?dockyard_plugin=pluginSalesforce&version=1";
            string url = Common.FormatAuthUrl(
                salesforceAuthUrl, Salesforce.Common.Models.ResponseTypes.Code,
                salesforceConsumerKey,
                HttpUtility.UrlEncode(redirectUrl), Salesforce.Common.Models.DisplayTypes.Page, false, HttpUtility.UrlEncode(exteranalStateValue), "full%20refresh_token"
                );
            return url;
        }
    }
}