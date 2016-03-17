using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using System.Collections.Generic;
using System.Linq;

namespace terminalSalesforce.Infrastructure
{
    public class Account : SalesforceObject
    {
        protected override bool ValidateObject(object salesforceObject)
        {
            //Account object related validation
            var accountObject = (AccountDTO) salesforceObject;
            if (accountObject == null || string.IsNullOrEmpty(accountObject.Name))
            {
                return false;
            }

            return true;
        }
    }
}