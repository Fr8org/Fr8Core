namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCreateDateToBaseDO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetRoles", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.AspNetUserRoles", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.Users", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.Emails", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.StoredFiles", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.Events", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.Attendees", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.EmailAddresses", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.Recipients", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.Negotiations", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.Calendars", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.Questions", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.Answers", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.Instructions", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.Profiles", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.ProfileNodes", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.ProfileItems", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.RemoteCalendarAuthData", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.RemoteCalendarProviders", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.CommunicationConfigurations", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.Envelopes", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.TrackingStatuses", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.UserAgentInfos", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.Incidents", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.Concepts", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.RemoteCalendarLinks", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.QuestionResponses", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.AuthorizationTokens", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.Logs", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
            AddColumn("dbo.NegotiationAnswerEmails", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NegotiationAnswerEmails", "CreateDate");
            DropColumn("dbo.Logs", "CreateDate");
            DropColumn("dbo.AuthorizationTokens", "CreateDate");
            DropColumn("dbo.QuestionResponses", "CreateDate");
            DropColumn("dbo.RemoteCalendarLinks", "CreateDate");
            DropColumn("dbo.Concepts", "CreateDate");
            DropColumn("dbo.Incidents", "CreateDate");
            DropColumn("dbo.UserAgentInfos", "CreateDate");
            DropColumn("dbo.TrackingStatuses", "CreateDate");
            DropColumn("dbo.Envelopes", "CreateDate");
            DropColumn("dbo.CommunicationConfigurations", "CreateDate");
            DropColumn("dbo.RemoteCalendarProviders", "CreateDate");
            DropColumn("dbo.RemoteCalendarAuthData", "CreateDate");
            DropColumn("dbo.ProfileItems", "CreateDate");
            DropColumn("dbo.ProfileNodes", "CreateDate");
            DropColumn("dbo.Profiles", "CreateDate");
            DropColumn("dbo.Instructions", "CreateDate");
            DropColumn("dbo.Answers", "CreateDate");
            DropColumn("dbo.Questions", "CreateDate");
            DropColumn("dbo.Calendars", "CreateDate");
            DropColumn("dbo.Negotiations", "CreateDate");
            DropColumn("dbo.Recipients", "CreateDate");
            DropColumn("dbo.EmailAddresses", "CreateDate");
            DropColumn("dbo.Attendees", "CreateDate");
            DropColumn("dbo.Events", "CreateDate");
            DropColumn("dbo.StoredFiles", "CreateDate");
            DropColumn("dbo.Emails", "CreateDate");
            DropColumn("dbo.Users", "CreateDate");
            DropColumn("dbo.AspNetUserRoles", "CreateDate");
            DropColumn("dbo.AspNetRoles", "CreateDate");
        }
    }
}
