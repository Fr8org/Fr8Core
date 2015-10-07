using Data.Interfaces.DataTransferObjects;
using Salesforce.Force;
using StructureMap;
using System;
using System.Threading.Tasks;
using terminal_Salesforce.Infrastructure;
using Utilities.Logging;

namespace terminal_Salesforce.Services
{
    public class Lead : ILead
    {
        ForceClient _forceClient;
        IConfiguration _salesforceAccount = ObjectFactory.GetInstance<IConfiguration>();

        public Lead()
        {
            _forceClient = _salesforceAccount.GetForceClient();
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
            var newLeadId = await _forceClient.CreateAsync("Lead", lead);
        }
    }
}