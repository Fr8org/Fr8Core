namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ImplementLastUpdated : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetRoles", "LastUpdated", c => c.DateTimeOffset(precision: 7));
            AddColumn("dbo.AspNetUserRoles", "LastUpdated", c => c.DateTimeOffset(precision: 7));
            AddColumn("dbo.Users", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.StoredFiles", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Events", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Attendees", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.EmailAddresses", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Recipients", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Negotiations", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Calendars", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Questions", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Answers", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Instructions", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Profiles", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.ProfileNodes", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.ProfileItems", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.RemoteCalendarAuthData", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.RemoteCalendarProviders", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.CommunicationConfigurations", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Envelopes", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.TrackingStatuses", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.UserAgentInfos", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Facts", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Incidents", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Concepts", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.RemoteCalendarLinks", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.QuestionResponses", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.AuthorizationTokens", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Logs", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Logs", "LastUpdated");
            DropColumn("dbo.AuthorizationTokens", "LastUpdated");
            DropColumn("dbo.QuestionResponses", "LastUpdated");
            DropColumn("dbo.RemoteCalendarLinks", "LastUpdated");
            DropColumn("dbo.Concepts", "LastUpdated");
            DropColumn("dbo.Incidents", "LastUpdated");
            DropColumn("dbo.Facts", "LastUpdated");
            DropColumn("dbo.UserAgentInfos", "LastUpdated");
            DropColumn("dbo.TrackingStatuses", "LastUpdated");
            DropColumn("dbo.Envelopes", "LastUpdated");
            DropColumn("dbo.CommunicationConfigurations", "LastUpdated");
            DropColumn("dbo.RemoteCalendarProviders", "LastUpdated");
            DropColumn("dbo.RemoteCalendarAuthData", "LastUpdated");
            DropColumn("dbo.ProfileItems", "LastUpdated");
            DropColumn("dbo.ProfileNodes", "LastUpdated");
            DropColumn("dbo.Profiles", "LastUpdated");
            DropColumn("dbo.Instructions", "LastUpdated");
            DropColumn("dbo.Answers", "LastUpdated");
            DropColumn("dbo.Questions", "LastUpdated");
            DropColumn("dbo.Calendars", "LastUpdated");
            DropColumn("dbo.Negotiations", "LastUpdated");
            DropColumn("dbo.Recipients", "LastUpdated");
            DropColumn("dbo.EmailAddresses", "LastUpdated");
            DropColumn("dbo.Attendees", "LastUpdated");
            DropColumn("dbo.Events", "LastUpdated");
            DropColumn("dbo.StoredFiles", "LastUpdated");
            DropColumn("dbo.Users", "LastUpdated");
            DropColumn("dbo.AspNetUserRoles", "LastUpdated");
            DropColumn("dbo.AspNetRoles", "LastUpdated");
        }
    }
}
