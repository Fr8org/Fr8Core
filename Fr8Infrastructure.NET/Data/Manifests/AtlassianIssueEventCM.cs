using Fr8.Infrastructure.Data.Constants;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class AtlassianIssueEventCM : Manifest
    {
        public string IssueId { get; set; }

        public string IssueKey { get; set; }
        public string UserId { get; set; }
        public string Time { get; set; }
        public string ChangedAspect { get; set; }
        public JiraIssueEventCM IssueEvent { get; set; }
        public AtlassianIssueEventCM(): base(MT.AtlassianIssueEvent)
        {

        }
    }
    public class JiraIssueEventCM
    {
        public string Timestamp { get; set; }
        public string WebhookEvent { get; set; }
        public string UserName { get; set; }
        public string IssueSummary { get; set; }
        public string IssueResolution { get; set; }
        public string ProjectName { get; set; }
        public string IssuePriority { get; set; }
        public string IssueAssigneeName { get; set; }
        public string IssueAssigneeEmailAddress { get; set; }
        public string IssueStatus { get; set; }
        public string IssueType { get; set; }
        public string Description { get; set; }
    }
}
