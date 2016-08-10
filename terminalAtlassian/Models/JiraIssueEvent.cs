using Atlassian.Jira;
using terminalAtlassian.Interfaces;

namespace terminalAtlassian.Models
{
    public class JiraIssueEvent
    {
        public string Timestamp { get; set; }

        public string WebhookEvent { get; set; }

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
        public IssueType IssueType { get; set; }

        public Project Project { get; set; }

        public IssueResolution Resolution { get; set; }

        public IssuePriority Priority { get; set; }

        public IssueLabels Labels { get; set; }
    }
}