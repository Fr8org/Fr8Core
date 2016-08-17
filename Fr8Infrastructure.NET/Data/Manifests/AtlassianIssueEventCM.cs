using Fr8.Infrastructure.Data.Constants;
using System;

namespace Fr8.Infrastructure.Data.Manifests
{
    public class AtlassianIssueEvent
    {
        public string IssueId { get; set; }
        public string IssueKey { get; set; }
        public string UserId { get; set; }
        public string Time { get; set; }
        public string ChangedAspect { get; set; }
        public string EventType { get; set; }
        public JiraIssueEvent IssueEvent { get; set; }
    }
    public class JiraIssueEvent
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
    public class JiraIssueWithCustomFieldsCM : Manifest
    {
        public AtlassianIssueEvent JiraIssue {get; set; }
        public JiraCustomField[] CustomFields { get; set; }
        public JiraComment[] Comments { get; set; }
        public JiraIssueWithCustomFieldsCM(): base(MT.AtlassianIssueEvent)
        {

        }
    }
    public class JiraCustomField
    {
        public string Key { get; set; }
        public string[] Values { get; set; }
    }

    public class JiraComment
    {
        public string Author { get; set; }
        public string Body { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string GroupLevel { get; set; }
        public string Id { get; set; }
        public string RoleLevel { get; set; }
        public string UpdateAuthor { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

}
