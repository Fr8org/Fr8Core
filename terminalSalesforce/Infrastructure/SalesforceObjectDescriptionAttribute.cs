using System;

namespace terminalSalesforce.Infrastructure
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class SalesforceObjectDescriptionAttribute : Attribute
    {
        public SalesforceObjectOperations AvailableOperations { get; set; }

        public SalesforceProperties Properties { get; set; }

        public SalesforceObjectDescriptionAttribute(SalesforceObjectOperations availableOperations, SalesforceProperties properties)
        {
            AvailableOperations = availableOperations;
            Properties = properties;
        }

        public SalesforceObjectDescriptionAttribute()
        {
        }
    }
}