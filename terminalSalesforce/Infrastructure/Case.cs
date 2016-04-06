namespace terminalSalesforce.Infrastructure
{
    public class Case : ISalesforceObject
    {
        public string AccountId { get; set; }
        public string CaseNumber { get; set; }
        public string ClosedDate { get; set; }
        public string CommunityId { get; set; }
        public string ConnectionReceivedId { get; set; }
        public string ConnectionSentId { get; set; }
        public string ContactId { get; set; }
        public string CreatorFullPhotoUrl { get; set; }
        public string CreatorName { get; set; }
        public string CreatorSmallPhotoUrl { get; set; }
        public string Description { get; set; }
        public string FeedItemId { get; set; }
        public bool HasCommentsUnreadByOwner { get; set; }
        public bool HasSelfServiceComments { get; set; }
        public bool IsClosed { get; set; }
        public bool IsClosedOnCreate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsEscalated { get; set; }
        public bool IsSelfServiceClosed { get; set; }
        public bool IsStopped { get; set; }
        public bool IsVisibleInSelfService { get; set; }
        public string LastReferencedDate { get; set; }
        public string LastViewedDate { get; set; }
        public string Origin { get; set; }
        public string OwnerId { get; set; }
        public string ParentId { get; set; }
        public string Priority { get; set; }
        public string QuestionId { get; set; }
        public string Reason { get; set; }
        public string RecordTypeId { get; set; }
        public string SlaStartDate { get; set; }
        public string Status { get; set; }
        public string StopStartDate { get; set; }
        public string Subject { get; set; }
        public string SuppliedCompany { get; set; }
        public string SuppliedEmail { get; set; }
        public string SuppliedName { get; set; }
        public string SuppliedPhone { get; set; }
        public string Type { get; set; }
        string ISalesforceObject.SalesforceObjectType => "Case";
        bool ISalesforceObject.Validate()
        {
            return !string.IsNullOrEmpty(AccountId);
        }
    }
}