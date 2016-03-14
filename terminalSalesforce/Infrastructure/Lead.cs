using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using System.Collections.Generic;
using System.Linq;

namespace terminalSalesforce.Infrastructure
{
    public class Lead : SalesforceObject
    {
        protected override bool ValidateObject(object salesforceObject)
        {
            //Lead object related validation
            var leadObject = (LeadDTO) salesforceObject;

            if (leadObject == null || string.IsNullOrEmpty(leadObject.LastName) ||
                string.IsNullOrEmpty(leadObject.Company))
            {
                return false;
            }

            return true;
        }
    }
}