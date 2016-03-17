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
    public class Order : SalesforceObject
    {
        protected override bool ValidateObject(object salesforceObject)
        {
            //Account object related validation
            var orderObject = (OrderDTO)salesforceObject;
            if (orderObject == null || string.IsNullOrEmpty(orderObject.AccountId))
            {
                return false;
            }

            return true;
        }
    }
}