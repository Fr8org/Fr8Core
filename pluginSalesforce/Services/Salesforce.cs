using Data.Interfaces.DataTransferObjects;
using pluginSalesforce.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using pluginSalesforce.sforce;
using System.Configuration;
using StructureMap;
using Utilities.Logging;

namespace pluginSalesforce.Services
{
    public class Salesforce : ISalesforce
    {   

        private SforceService binding;
        ISalesforceAccount _salesforceAccount = ObjectFactory.GetInstance<ISalesforceAccount>();

        public Salesforce()
        {
            binding = _salesforceAccount.GetSalesForceServiceBinding();
        }

        public bool CreateLead(ActionDTO currentDTO)
        {
            bool createFlag = true;
            try
            {

                //Hardcoded For Testing Should be Removed 
                Lead newLead = new Lead();
                newLead.FirstName = "Moble";
                newLead.LastName = "Logiticks";
                newLead.Email = "dev@logiticks.com";
                newLead.Title = "test";
                newLead.Company = "Logiticks.com";

            
            AssignmentRuleHeader arh = new AssignmentRuleHeader();
            arh.useDefaultRule = true;

            binding.AssignmentRuleHeaderValue = arh;
            // Every operation that results in a new or updated lead will use the
            // specified rule until the header is removed from the proxy binding

            
            SaveResult[] sr = binding.create(new sObject[] { newLead });
            if(sr[0] !=null)
            {
                if (sr[0].success)
                {
                    createFlag = true;
                }
                else
                {
                    createFlag = false;
                }
            }
            

            // This call effectively removes the header. The next lead will be assigned
            // to the default lead owner.

            binding.AssignmentRuleHeaderValue = null;
            }
            catch (Exception ex)
            {
                Logger.GetLogger().Error(ex);
            }
            return createFlag;
        }
    }
}