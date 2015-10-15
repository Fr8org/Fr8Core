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

namespace pluginSalesforce.Services
{
    public class SalesforceIntegration : ISalesforceIntegration
    {
        ForceClient forceClient;
        //IConfiguration _salesforceAccount = ObjectFactory.GetInstance<IConfiguration>();
        string salesforceConsumerKey = string.Empty;
        string salesforceAuthUrl = string.Empty;
        string salesforceAuthCallBackUrl = string.Empty;
        string salesforceConsumerSecret = string.Empty;
        string tokenRequestEndpointUrl = string.Empty;

        public SalesforceIntegration()
        {
            //forceClient = _salesforceAccount.GetForceClient();
            salesforceConsumerKey = CloudConfigurationManager.GetSetting("SalesforceConsumerKey");
            salesforceAuthUrl = CloudConfigurationManager.GetSetting("SalesforceAuthURL");
            salesforceAuthCallBackUrl = CloudConfigurationManager.GetSetting("SalesforceAuthCallbackURL");
            salesforceConsumerKey = CloudConfigurationManager.GetSetting("SalesforceConsumerKey");
            salesforceConsumerSecret = CloudConfigurationManager.GetSetting("SalesforceConsumerSecret");
            tokenRequestEndpointUrl = CloudConfigurationManager.GetSetting("tokenRequestEndpointUrl");
        }

        public bool CreateLead(ActionDTO currentDTO)
        {
            bool createFlag = true;
            try
            {
                var createtask = CreateLead();
                createtask.Wait();
            }
            catch (Exception ex)
            {
                createFlag = false;
                Logger.GetLogger().Error(ex);
            }
            return createFlag;
        }

        public async Task<string> GetAuthToken(string code)
        {
            string redirectUrl = "https://7c24ff2d.ngrok.io/ExternalAuth/AuthSuccess?dockyard_plugin=pluginSalesforce&version=1";
            salesforceAuthCallBackUrl = redirectUrl;
            var auth = new AuthenticationClient();
            await auth.WebServerAsync(salesforceConsumerKey, salesforceConsumerSecret, salesforceAuthCallBackUrl, code, tokenRequestEndpointUrl);
            return auth.AccessToken;
        }

        private async Task CreateLead()
        {
            LeadDTO lead = new LeadDTO();
            lead.FirstName = "Moble-FirstName";
            lead.LastName = "LastName";
            lead.Company = "Logiticks";
            lead.Title = "Title -1";
            var newLeadId = await forceClient.CreateAsync("Lead", lead);
        }

        public string CreateAuthUrl()
        {
            string redirectUrl = "https://7c24ff2d.ngrok.io/ExternalAuth/AuthSuccess?dockyard_plugin=pluginSalesforce&version=1";
            salesforceAuthCallBackUrl = redirectUrl;
            string url = Common.FormatAuthUrl(
                salesforceAuthUrl,Salesforce.Common.Models.ResponseTypes.Code,
                salesforceConsumerKey,
                HttpUtility.UrlEncode(salesforceAuthCallBackUrl)
                );
            return url;
        }

        public async Task<string> GetOAuthToken(string code)
        {
            //var template = CloudConfigurationManager.GetSetting("SlackOAuthAccessUrl");
            //var url = template.Replace("%CODE%", code);

            //var httpClient = new HttpClient();
            //using (var response = await httpClient.GetAsync(url))
            //{
            //    var responseString = await response.Content.ReadAsStringAsync();
            //    var jsonObj = JsonConvert.DeserializeObject<JObject>(responseString);

            //    return jsonObj.Value<string>("access_token");
            //}

            return "";
        }


        private string PrepareTokenUrl(string urlKey, string oauthToken)
        {
            var template = CloudConfigurationManager.GetSetting(urlKey);
            var url = template.Replace("%TOKEN%", oauthToken);

            return url;
        }

        public async Task<string> GetUserId(string oauthToken)
        {
            //var url = PrepareTokenUrl("SlackAuthTestUrl", oauthToken);

            //var httpClient = new HttpClient();
            //using (var response = await httpClient.GetAsync(url))
            //{
            //    var responseString = await response.Content.ReadAsStringAsync();
            //    var jsonObj = JsonConvert.DeserializeObject<JObject>(responseString);

            //    return jsonObj.Value<string>("user_id");
            //}

            return "";
        }
    }
}