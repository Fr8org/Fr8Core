using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace terminalSalesforce.Infrastructure
{
    public class OrderDTO
    {
        public string AccountId { get; set; }
        public string ActivatedById { get; set; }
        public string ActivatedDate { get; set; }
        public string BillingCity { get; set; }
        public string BillingCountry { get; set; }
        public string BillingCountryCode { get; set; }
        public double BillingLatitude { get; set; }
        public double BillingLongitude { get; set; }
        public string BillingPostalCode { get; set; }
        public string BillingState { get; set; }
        public string BillingStateCode { get; set; }
        public string BillingStreet { get; set; }
        public string BillToContactId { get; set; }
        public string CompanyAuthorizedById { get; set; }
        public string CompanyAuthorizedDate { get; set; }
        public string ContractId { get; set; }
        public string CustomerAuthorizedById { get; set; }
        public string CustomerAuthorizedDate { get; set; }
        public string Description { get; set; }
        public string EffectiveDate { get; set; }
        public string EndDate { get; set; }
        public bool IsReductionOrder { get; set; }
        public string LastReferencedDate { get; set; }
        public string LastViewedDate { get; set; }
        public string Name { get; set; }
        public string OpportunityId { get; set; }
        public string OrderNumber { get; set; }
        public string OrderReferenceNumber { get; set; }
        public string OriginalOrderId { get; set; }
        public string OwnerId { get; set; }
        public string PoDate { get; set; }
        public string PoNumber { get; set; }
        public string Pricebook2Id { get; set; }
        public string QuoteId { get; set; }
        public string RecordTypeId { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingCountryCode { get; set; }
        public double ShippingLatitude { get; set; }
        public double ShippingLongitude { get; set; }
        public string ShippingPostalCode { get; set; }
        public string ShippingState { get; set; }
        public string ShippingStateCode { get; set; }
        public string ShippingStreet { get; set; }
        public string ShipToContactId { get; set; }
        public string Status { get; set; }
        public string StatusCode { get; set; }
        public string TotalAmount { get; set; }
        public string Type { get; set; }

    }
}