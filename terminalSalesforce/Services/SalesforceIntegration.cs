using System;
using System.Threading.Tasks;
using Data.Interfaces.DataTransferObjects;
using terminalSalesforce.Infrastructure;
using Utilities.Logging;

namespace terminalSalesforce.Services
{
    public class SalesforceIntegration : ISalesforceIntegration
    {
        private Authentication _authentication = new Authentication();
        private Lead _lead = new Lead();

        public SalesforceIntegration()
        {
           
        }

        public bool CreateLead(ActionDTO currentDTO)
        {
            bool createFlag = true;
            try
            {
                var actionDTO = Task.Run(() => _authentication.RefreshAccessToken(currentDTO)).Result;
                currentDTO = actionDTO;
                var createtask = _lead.CreateLead(currentDTO);
            }
            catch (Exception ex)
            {
                createFlag = false;
                Logger.GetLogger().Error(ex);
                throw;
            }
            return createFlag;
        }         
    }
}