namespace terminalSalesforce.Infrastructure
{
    public class Product : ISalesforceObject
    {
        public bool CanUseQuantitySchedule { get; set; }
        public bool CanUseRevenueSchedule { get; set; }
        public string ConnectionReceivedId { get; set; }
        public string ConnectionSentId { get; set; }
        public string CurrencyIsoCode { get; set; }
        public string DefaultPrice { get; set; }
        public string Description { get; set; }
        public string Family { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string LastReferencedDate { get; set; }
        public string LastViewedDate { get; set; }
        public string Name { get; set; }
        public int NumberOfQuantityInstallments { get; set; }
        public int NumberOfRevenueInstallments { get; set; }
        public string ProductCode { get; set; }
        public string QuantityInstallmentPeriod { get; set; }
        public string QuantityScheduleType { get; set; }
        public bool RecalculateTotalPrice { get; set; }
        public string RevenueInstallmentPeriod { get; set; }
        public string RevenueScheduleType { get; set; }
        string ISalesforceObject.SalesforceObjectType => "Product2";

        bool ISalesforceObject.Validate()
        {
            return !string.IsNullOrEmpty(Name);
        }
    }
}