using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalSalesforce.Infrastructure
{
    public class Opportunity : SalesforceObject
    {
        protected override bool ValidateObject(object salesforceObject)
        {
            //Account object related validation
            var opportunityObject = (OpportunityDTO)salesforceObject;
            if (opportunityObject == null || string.IsNullOrEmpty(opportunityObject.Name))
            {
                return false;
            }

            return true;
        }
    }
}