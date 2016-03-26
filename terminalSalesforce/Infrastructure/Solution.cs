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
    public class Solution : SalesforceObject
    {
        protected override bool ValidateObject(object salesforceObject)
        {
            //Account object related validation
            var solutionObject = (SolutionDTO)salesforceObject;
            if (solutionObject == null || string.IsNullOrEmpty(solutionObject.SolutionName))
            {
                return false;
            }

            return true;
        }
    }
}