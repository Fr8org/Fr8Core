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

namespace pluginSalesforce.Services
{
    public class SalesforceIntegration : ISalesforceIntegration
    {
        ForceClient forceClient;
        //IConfiguration _salesforceAccount = ObjectFactory.GetInstance<IConfiguration>();
        string salesforceConsumerKey = string.Empty;
        string salesforceAuthUrl = string.Empty;
        string salesforceAuthCallBackUrl = string.Empty;

        public SalesforceIntegration()
        {
            //forceClient = _salesforceAccount.GetForceClient();
            salesforceConsumerKey = CloudConfigurationManager.GetSetting("SalesforceConsumerKey");
            salesforceAuthUrl = CloudConfigurationManager.GetSetting("SalesforceAuthURL");
            salesforceAuthCallBackUrl = CloudConfigurationManager.GetSetting("SalesforceAuthCallbackURL");
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
            string url = Common.FormatAuthUrl(
                salesforceAuthUrl,Salesforce.Common.Models.ResponseTypes.Code,
                salesforceConsumerKey,
                HttpUtility.UrlEncode(salesforceAuthCallBackUrl)
                );
            return url;
        }
    }
}