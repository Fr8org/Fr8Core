using pluginSalesforce.Infrastructure;
using Salesforce.Common;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

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
           this.salesforceUserName = ConfigurationManager.AppSettings["SalesforceUserName"];
           this.salesforcePassword = ConfigurationManager.AppSettings["SalesforcePassword"];
           this.salesforceConsumerKey = ConfigurationManager.AppSettings["SalesforceConsumerKey"];
           this.salesforceConsumerSecret = ConfigurationManager.AppSettings["SalesforceConsumerSecret"];

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