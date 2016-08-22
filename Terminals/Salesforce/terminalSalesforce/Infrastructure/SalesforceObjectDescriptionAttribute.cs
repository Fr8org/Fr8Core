using System;

namespace terminalSalesforce.Infrastructure
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class SalesforceObjectDescriptionAttribute : Attribute
    {
        public SalesforceObjectOperations AvailableOperations { get; set; }

        public SalesforceObjectProperties AvailableProperties { get; set; }

        public SalesforceObjectDescriptionAttribute(SalesforceObjectOperations availableOperations, SalesforceObjectProperties availableProperties)
        {
            AvailableOperations = availableOperations;
            AvailableProperties = availableProperties;
        }

        public SalesforceObjectDescriptionAttribute()
        {
        }
    }
}