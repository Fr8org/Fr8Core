using Data.Interfaces.DataTransferObjects;
using Data.Interfaces.Manifests;
using Newtonsoft.Json.Linq;
using Salesforce.Common.Models;
using System.Collections.Generic;
using System.Linq;

namespace terminalSalesforce.Infrastructure
{
    public class Contact : SalesforceObject
    {
        protected override bool ValidateObject(object salesforceObject)
        {
            //Contact object related validation
            var contactObject = (ContactDTO) salesforceObject;

            if (contactObject == null || string.IsNullOrEmpty(contactObject.LastName))
            {
                return false;
            }

            return true;
        }
    }
}