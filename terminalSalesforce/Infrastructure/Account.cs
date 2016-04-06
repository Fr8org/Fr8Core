namespace terminalSalesforce.Infrastructure
{
    public class Account : ISalesforceObject
    {
        public string Name { get; set; }
        public string AccountNumber { get; set; }
        public string Phone { get; set; }
        public string Site { get; set; }
        public string Fax { get; set; }
        public string Website { get; set; }
        public string BillingStreet { get; set; }
        public string BillingCity { get; set; }
        public string BillingState { get; set; }
        public string BillingPostalCode { get; set; }
        public string BillingCountry { get; set; }
        public string ShippingStreet { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingPostalCode { get; set; }
        public string ShippingCountry { get; set; }

        string ISalesforceObject.SalesforceObjectType => "Account";

        bool ISalesforceObject.Validate()
        {
            return !string.IsNullOrEmpty(Name);
        }
    }
}