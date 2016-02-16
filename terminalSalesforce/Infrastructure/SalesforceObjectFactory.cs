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
                return new List<string> { "Account",
                                          "Lead",
                                          "Contact",
                                          "Opportunity",
                                          //"Forecasts",
                                          "Contract",
                                          "Order",
                                          "Case",
                                          "Solution",
                                          "Product",
                                          "Document"
                                          //"File"
                                        };
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
                case "Opportunity":
                    salesforceObject = new Opportunity();
                    break;
                //case "Forecasts":
                //    salesforceObject = new Forecasts();
                //    break;
                case "Contract":
                    salesforceObject = new Contract();
                    break;
                case "Order":
                    salesforceObject = new Order();
                    break;
                case "Case":
                    salesforceObject = new Case();
                    break;
                case "Solution":
                    salesforceObject = new Solution();
                    break;
                case "Product":
                    salesforceObject = new Product();
                    break;
                case "Document":
                    salesforceObject = new Document();
                    break;
                //case "File":
                //    salesforceObject = new File();
                //    break;
            }

            return salesforceObject;
        }
    }
}