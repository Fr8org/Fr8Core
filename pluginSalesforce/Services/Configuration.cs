using Salesforce.Common;
using Salesforce.Force;
using System.Configuration;
using System.Threading.Tasks;
using terminal_Salesforce.Infrastructure;

namespace terminal_Salesforce.Services
{
    public class Configuration:IConfiguration
    {
       
        public readonly string _salesforceUserName;
        public readonly string _salesforcePassword;
        public readonly string _salesforceConsumerKey;
        public readonly string _salesforceConsumerSecret;
        ForceClient forceClient = null;
        AuthenticationClient authclient;

        public Configuration()
        {
           this._salesforceUserName = ConfigurationManager.AppSettings["SalesforceUserName"];
           this._salesforcePassword = ConfigurationManager.AppSettings["SalesforcePassword"];
           this._salesforceConsumerKey = ConfigurationManager.AppSettings["SalesforceConsumerKey"];
           this._salesforceConsumerSecret = ConfigurationManager.AppSettings["SalesforceConsumerSecret"];

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
            await authclient.UsernamePasswordAsync(_salesforceConsumerKey, _salesforceConsumerSecret, _salesforceUserName, _salesforcePassword);
        }
        
        public ForceClient GetForceClient()
        {
            return forceClient;
        }
    }
}