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

namespace pluginSalesforce.Services
{
    public class Salesforce : ISalesforce
    {
        ForceClient forceClient;
        ISalesforceAccount _salesforceAccount = ObjectFactory.GetInstance<ISalesforceAccount>();

        public Salesforce()
        {
            forceClient = _salesforceAccount.GetForceClient();
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
            Lead lead = new Lead();
            lead.FirstName = "Moble-FirstName";
            lead.LastName = "LastName";
            lead.Company = "Logiticks";
            lead.Title = "Title -1";
            var newLeadId = await forceClient.CreateAsync("Lead", lead);
        }
    }
}