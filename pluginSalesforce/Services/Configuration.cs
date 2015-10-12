using pluginSalesforce.Infrastructure;
using Salesforce.Common;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using fr8.Microsoft.Azure;

namespace pluginSalesforce.Services
{
    public class Configuration:IConfiguration
    {
       
        public readonly string salesforceUserName;
        public readonly string salesforcePassword;
        public readonly string salesforceConsumerKey;
        public readonly string salesforceConsumerSecret;
        ForceClient forceClient = null;
        AuthenticationClient authclient;

        public Configuration()
        {
           this.salesforceUserName = CloudConfigurationManager.GetSetting("SalesforceUserName");
           this.salesforcePassword = CloudConfigurationManager.GetSetting("SalesforcePassword");
           this.salesforceConsumerKey = CloudConfigurationManager.GetSetting("SalesforceConsumerKey");
           this.salesforceConsumerSecret = CloudConfigurationManager.GetSetting("SalesforceConsumerSecret");

           authclient = new AuthenticationClient();
           var connectionTask= GetConnection();
           connectionTask.Wait();

           var instanceUrl = authclient.InstanceUrl;
           var accessToken = authclient.AccessToken;
           var apiVersion = authclient.ApiVersion;

           forceClient = new ForceClient(instanceUrl, accessToken, apiVersion);
        }

        private async Task GetConnection()
        {
            await authclient.UsernamePasswordAsync(salesforceConsumerKey, salesforceConsumerSecret, salesforceUserName, salesforcePassword);
        }
        
        public ForceClient GetForceClient()
        {
            return forceClient;
        }
    }
}