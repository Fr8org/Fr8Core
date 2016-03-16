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
    public class Document : SalesforceObject
    {
        protected override bool ValidateObject(object salesforceObject)
        {
            //Account object related validation
            var documentObject = (DocumentDTO)salesforceObject;
            if (documentObject == null || string.IsNullOrEmpty(documentObject.Name))
            {
                return false;
            }

            return true;
        }
    }
}