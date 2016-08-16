using Atlassian.Jira;
using Newtonsoft.Json;

namespace terminalAtlassian.Models
{
    public class JiraIssueEvent
    {
        public string Timestamp { get; set; }

        public string WebhookEvent { get; set; }

        [JsonProperty("issue_event_type_name")]
        public string IssueEventTypeName { get; set; }

        public JiraUser User { get; set; }

        public JiraIssue Issue { get; set; }
    }
    public class JiraIssue
    {

        public string Id { get; set; }

        public string Self { get; set; }

        public string Key { get; set; }

        public IssueFields Fields { get; set; }
    }
    public class IssueFields
    {
        public string Summary { get; set; }

        public string Resolution { get; set; }

        public IssueProject Project { get; set; }

        public IssuePriority Priority { get; set; }

        public IssueAssignee Assignee { get; set; }

        public IssueStatus Status { get; set; }

        public IssueType IssueType { get; set; }

        public string Description { get; set; }
    }
    public class IssueType
    {
        public string Name { get; set; }

        public bool SubTask { get; set; }
    }
    public class IssueProject
    {
        public string Name { get; set; }

        public string Key { get; set; }
    }

    public class IssuePriority
    {
        public string Name { get; set; }
    }

    public class IssueAssignee
    {
        public string Name { get; set; }
        public string EmailAddress { get; set; }

        public string DisplayName { get; set; }

    }

    public class IssueStatus
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}