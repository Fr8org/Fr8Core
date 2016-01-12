using System;
using System.Collections.Generic;

namespace terminalSalesforce.Infrastructure
{
    /// <summary>
    /// Factory to create Salesforce object types
    /// </summary>
    public class SalesforceObjectFactory
    {
        private IList<string> SupportedSalesforceObjects
        {
            get
            {
                return new List<string> { "Account", "Lead", "Contact" };
            }
        }

        public SalesforceObject GetSalesforceObject(string objectName)
        {
            if (!SupportedSalesforceObjects.Contains(objectName))
            {
                throw new NotSupportedException(
                    string.Format("Not Supported Salesforce object name {0} has been given for getting objects.",
                        objectName));
            }

            SalesforceObject salesforceObject = null;

            switch (objectName)
            {
                case "Account":
                    salesforceObject = new Account();
                    break;
                case "Lead":
                    salesforceObject = new Lead();
                    break;
                case "Contact":
                    salesforceObject = new Contact();
                    break;
            }

            return salesforceObject;
        }
    }
}