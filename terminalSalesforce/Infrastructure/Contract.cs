namespace terminalSalesforce.Infrastructure
{
    public class Contract : ISalesforceObject
    {
        public string AccountId { get; set; }
        public string ActivatedById { get; set; }
        public string ActivatedDate { get; set; }
        public string BillingAddress { get; set; }
        public string BillingCity { get; set; }
        public string BillingCountry { get; set; }
        public string BillingCountryCode { get; set; }
        public double BillingLatitude { get; set; }
        public double BillingLongitude { get; set; }
        public string BillingPostalCode { get; set; }
        public string BillingState { get; set; }
        public string BillingStateCode { get; set; }
        public string BillingStreet { get; set; }
        public string CompanySignedDate { get; set; }
        public string CompanySignedId { get; set; }
        public string ContractNumber { get; set; }
        public int ContractTerm { get; set; }
        public string CustomerSignedDate { get; set; }
        public string CustomerSignedId { get; set; }
        public string CustomerSignedTitle { get; set; }
        public string Description { get; set; }
        public string EndDate { get; set; }
        public bool IsDeleted { get; set; }
        public string LastActivityDate { get; set; }
        public string LastApprovedDate { get; set; }
        public string LastReferencedDate { get; set; }
        public string LastViewedDate { get; set; }
        public string OwnerExpirationNotice { get; set; }
        public string OwnerId { get; set; }
        public string Pricebook2Id { get; set; }
        public string RecordTypeId { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingCountryCode { get; set; }
        public double ShippingLatitude { get; set; }
        public double ShippingLongitude { get; set; }
        public string ShippingPostalCode { get; set; }
        public string ShippingState { get; set; }
        public string ShippingStateCode { get; set; }
        public string ShippingStreet { get; set; }
        public string SpecialTerms { get; set; }
        public string StartDate { get; set; }
        public string Status { get; set; }
        public string StatusCode { get; set; }
        string ISalesforceObject.SalesforceObjectType => "Contract";
        bool ISalesforceObject.Validate()
        {
            return !string.IsNullOrEmpty(AccountId);
        }
    }
}