namespace terminalSalesforce.Infrastructure
{
    public class Opportunity : ISalesforceObject
    {
        public string AccountId { get; set; }
        public int Amount { get; set; }
        public string CampaignId { get; set; }
        public string CloseDate { get; set; }
        public string ConnectionReceivedId { get; set; }
        public string ConnectionSentId { get; set; }
        public string ContractId { get; set; }
        public string CurrencyIsoCode { get; set; }
        public string Description { get; set; }
        public string ExpectedRevenue { get; set; }
        public string Fiscal { get; set; }
        public int FiscalQuarter { get; set; }
        public int FiscalYear { get; set; }
        public string ForecastCategory { get; set; }
        public string ForecastCategoryName { get; set; }
        public bool HasOpenActivity { get; set; }
        public bool HasOpportunityLineItem { get; set; }
        public bool HasOverdueTask { get; set; }
        public bool IsClosed { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsExcludedFromTerritory2Filter { get; set; }
        public bool IsSplit { get; set; }
        public bool IsWon { get; set; }
        public bool LastActivityDate { get; set; }
        public string LastReferencedDate { get; set; }
        public string LastViewedDate { get; set; }
        public string LeadSource { get; set; }
        public string Name { get; set; }
        public string NextStep { get; set; }
        public string OwnerId { get; set; }
        public string Pricebook2Id { get; set; }
        public string PricebookId { get; set; }
        public string Probability { get; set; }
        public string RecordTypeId { get; set; }
        public string StageName { get; set; }
        public string SyncedQuoteID { get; set; }
        public string Territory2Id { get; set; }
        public double TotalOpportunityQuantity { get; set; }
        public string Type { get; set; }
        string ISalesforceObject.SalesforceObjectType => "Opportunity";
        bool ISalesforceObject.Validate()
        {
            return !string.IsNullOrEmpty(Name);
        }
    }
}