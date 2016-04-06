namespace terminalSalesforce.Infrastructure
{
    public class Lead : ISalesforceObject
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string Title { get; set; }
        public string Phone { get; set; }
        public string MobilePhone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string Description { get; set; }
        string ISalesforceObject.SalesforceObjectType => "Lead";
        bool ISalesforceObject.Validate()
        {
            return !string.IsNullOrEmpty(LastName) && !string.IsNullOrEmpty(Company);
        }
    }
}