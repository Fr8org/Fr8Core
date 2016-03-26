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
    public class Contract : SalesforceObject
    {
        protected override bool ValidateObject(object salesforceObject)
        {
            //Account object related validation
            var contractObject = (ContractDTO)salesforceObject;
            if (contractObject == null || string.IsNullOrEmpty(contractObject.AccountId))
            {
                return false;
            }

            return true;
        }
    }
}