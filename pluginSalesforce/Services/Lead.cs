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
    public class Lead : ILead
    {
        ForceClient forceClient;
        IConfiguration _salesforceAccount = ObjectFactory.GetInstance<IConfiguration>();

        public Lead()
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
            LeadDTO lead = new LeadDTO();
            lead.FirstName = "Moble-FirstName";
            lead.LastName = "LastName";
            lead.Company = "Logiticks";
            lead.Title = "Title -1";
            var newLeadId = await forceClient.CreateAsync("Lead", lead);
        }
    }
}