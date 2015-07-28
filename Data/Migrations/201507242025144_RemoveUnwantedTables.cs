namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveUnwantedTables : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Attendees", "EmailAddressID", "dbo.EmailAddresses");
            DropForeignKey("dbo.Attendees", "NegotiationID", "dbo.Negotiations");
            DropForeignKey("dbo.Events", "CalendarID", "dbo.Calendars");
            DropForeignKey("dbo.Calendars", "NegotiationID", "dbo.Negotiations");
            DropForeignKey("dbo.Negotiations", "NegotiationState", "dbo._NegotiationStateTemplate");
            DropForeignKey("dbo.Answers", "AnswerStatus", "dbo._AnswerStatusTemplate");
            DropForeignKey("dbo.Answers", "EventID", "dbo.Events");
            DropForeignKey("dbo.Answers", "UserID", "dbo.Users");
            DropForeignKey("dbo.Answers", "QuestionID", "dbo.Questions");
            DropForeignKey("dbo.Questions", "CalendarID", "dbo.Calendars");
            DropForeignKey("dbo.Questions", "QuestionStatus", "dbo._QuestionStatusTemplate");
            DropForeignKey("dbo.Questions", "NegotiationId", "dbo.Negotiations");
            DropForeignKey("dbo.Attendees", "ParticipationStatus", "dbo._ParticipationStatusTemplate");
            DropForeignKey("dbo.Attendees", "EventID", "dbo.Events");
            DropForeignKey("dbo.Events", "BookingRequestID", "dbo.BookingRequests");
            DropForeignKey("dbo.Events", "CreatedByID", "dbo.Users");
            DropForeignKey("dbo.Events", "CreateType", "dbo._EventCreateTypeTemplate");
            DropForeignKey("dbo.Events", "EventStatus", "dbo._EventStatusTemplate");
            DropForeignKey("dbo.Events", "SyncStatus", "dbo._EventSyncStatusTemplate");
            DropForeignKey("dbo.EventEmail", "EmailID", "dbo.Emails");
            DropForeignKey("dbo.EventEmail", "EventID", "dbo.Events");
            DropForeignKey("dbo.BookingRequestCalendar", "BookingRequestID", "dbo.BookingRequests");
            DropForeignKey("dbo.BookingRequestCalendar", "CalendarID", "dbo.Calendars");
            DropForeignKey("dbo.Emails", "ConversationId", "dbo.BookingRequests");
            DropForeignKey("dbo.BookingRequestInstruction", "BookingRequestID", "dbo.BookingRequests");
            DropForeignKey("dbo.BookingRequestInstruction", "InstructionID", "dbo.Instructions");
            DropForeignKey("dbo.Negotiations", "BookingRequestID", "dbo.BookingRequests");
            DropForeignKey("dbo.Calendars", "OwnerID", "dbo.Users");
            DropForeignKey("dbo.RemoteCalendarLinks", "LocalCalendarID", "dbo.Calendars");
            DropForeignKey("dbo.RemoteCalendarLinks", "ProviderID", "dbo.RemoteCalendarProviders");
            DropForeignKey("dbo.QuestionResponses", "AnswerID", "dbo.Answers");
            DropForeignKey("dbo.QuestionResponses", "UserID", "dbo.Users");
            DropForeignKey("dbo.NegotiationAnswerEmails", "EmailID", "dbo.Emails");
            DropForeignKey("dbo.NegotiationAnswerEmails", "NegotiationID", "dbo.Negotiations");
            DropForeignKey("dbo.NegotiationAnswerEmails", "UserID", "dbo.Users");
            DropForeignKey("dbo.BookingRequests", "Id", "dbo.Emails");
            DropForeignKey("dbo.BookingRequests", "CustomerID", "dbo.Users");
            DropForeignKey("dbo.BookingRequests", "State", "dbo._BookingRequestStateTemplate");
            DropForeignKey("dbo.BookingRequests", "BookerID", "dbo.Users");
            DropForeignKey("dbo.BookingRequests", "PreferredBookerID", "dbo.Users");
            DropForeignKey("dbo.BookingRequests", "Availability", "dbo._BookingRequestAvailabilityTemplate");
            DropForeignKey("dbo.InvitationResponses", "Id", "dbo.Emails");
            DropForeignKey("dbo.InvitationResponses", "AttendeeId", "dbo.Attendees");
            DropIndex("dbo.Emails", new[] { "ConversationId" });
            DropIndex("dbo.Events", new[] { "EventStatus" });
            DropIndex("dbo.Events", new[] { "CreatedByID" });
            DropIndex("dbo.Events", new[] { "CalendarID" });
            DropIndex("dbo.Events", new[] { "BookingRequestID" });
            DropIndex("dbo.Events", new[] { "CreateType" });
            DropIndex("dbo.Events", new[] { "SyncStatus" });
            DropIndex("dbo.Attendees", new[] { "EmailAddressID" });
            DropIndex("dbo.Attendees", new[] { "EventID" });
            DropIndex("dbo.Attendees", new[] { "NegotiationID" });
            DropIndex("dbo.Attendees", new[] { "ParticipationStatus" });
            DropIndex("dbo.Negotiations", new[] { "NegotiationState" });
            DropIndex("dbo.Negotiations", new[] { "BookingRequestID" });
            DropIndex("dbo.Calendars", new[] { "OwnerID" });
            DropIndex("dbo.Calendars", new[] { "NegotiationID" });
            DropIndex("dbo.Questions", new[] { "QuestionStatus" });
            DropIndex("dbo.Questions", new[] { "CalendarID" });
            DropIndex("dbo.Questions", new[] { "NegotiationId" });
            DropIndex("dbo.Answers", new[] { "QuestionID" });
            DropIndex("dbo.Answers", new[] { "EventID" });
            DropIndex("dbo.Answers", new[] { "AnswerStatus" });
            DropIndex("dbo.Answers", new[] { "UserID" });
            DropIndex("dbo.RemoteCalendarLinks", new[] { "LocalCalendarID" });
            DropIndex("dbo.RemoteCalendarLinks", new[] { "ProviderID" });
            DropIndex("dbo.QuestionResponses", new[] { "AnswerID" });
            DropIndex("dbo.QuestionResponses", new[] { "UserID" });
            DropIndex("dbo.NegotiationAnswerEmails", new[] { "EmailID" });
            DropIndex("dbo.NegotiationAnswerEmails", new[] { "NegotiationID" });
            DropIndex("dbo.NegotiationAnswerEmails", new[] { "UserID" });
            DropIndex("dbo.EventEmail", new[] { "EmailID" });
            DropIndex("dbo.EventEmail", new[] { "EventID" });
            DropIndex("dbo.BookingRequestCalendar", new[] { "BookingRequestID" });
            DropIndex("dbo.BookingRequestCalendar", new[] { "CalendarID" });
            DropIndex("dbo.BookingRequestInstruction", new[] { "BookingRequestID" });
            DropIndex("dbo.BookingRequestInstruction", new[] { "InstructionID" });
            DropIndex("dbo.BookingRequests", new[] { "Id" });
            DropIndex("dbo.BookingRequests", new[] { "CustomerID" });
            DropIndex("dbo.BookingRequests", new[] { "State" });
            DropIndex("dbo.BookingRequests", new[] { "BookerID" });
            DropIndex("dbo.BookingRequests", new[] { "PreferredBookerID" });
            DropIndex("dbo.BookingRequests", new[] { "Availability" });
            DropIndex("dbo.InvitationResponses", new[] { "Id" });
            DropIndex("dbo.InvitationResponses", new[] { "AttendeeId" });
            DropColumn("dbo.Emails", "ConversationId");
            DropTable("dbo.Events");
            DropTable("dbo.Attendees");
            DropTable("dbo.Negotiations");
            DropTable("dbo.Calendars");
            DropTable("dbo._NegotiationStateTemplate");
            DropTable("dbo.Questions");
            DropTable("dbo.Answers");
            DropTable("dbo._AnswerStatusTemplate");
            DropTable("dbo._QuestionStatusTemplate");
            DropTable("dbo._ParticipationStatusTemplate");
            DropTable("dbo._EventCreateTypeTemplate");
            DropTable("dbo._EventSyncStatusTemplate");
            DropTable("dbo._BookingRequestAvailabilityTemplate");
            DropTable("dbo._BookingRequestStateTemplate");
            DropTable("dbo.RemoteCalendarLinks");
            DropTable("dbo.QuestionResponses");
            DropTable("dbo.NegotiationAnswerEmails");
            DropTable("dbo.EventEmail");
            DropTable("dbo.BookingRequestCalendar");
            DropTable("dbo.BookingRequestInstruction");
            DropTable("dbo.BookingRequests");
            DropTable("dbo.InvitationResponses");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.InvitationResponses",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        AttendeeId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BookingRequests",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        CustomerID = c.String(nullable: false, maxLength: 128),
                        State = c.Int(nullable: false),
                        BookerID = c.String(maxLength: 128),
                        PreferredBookerID = c.String(maxLength: 128),
                        Availability = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BookingRequestInstruction",
                c => new
                    {
                        BookingRequestID = c.Int(nullable: false),
                        InstructionID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.BookingRequestID, t.InstructionID });
            
            CreateTable(
                "dbo.BookingRequestCalendar",
                c => new
                    {
                        BookingRequestID = c.Int(nullable: false),
                        CalendarID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.BookingRequestID, t.CalendarID });
            
            CreateTable(
                "dbo.EventEmail",
                c => new
                    {
                        EmailID = c.Int(nullable: false),
                        EventID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.EmailID, t.EventID });
            
            CreateTable(
                "dbo.NegotiationAnswerEmails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EmailID = c.Int(nullable: false),
                        NegotiationID = c.Int(nullable: false),
                        UserID = c.String(nullable: false, maxLength: 128),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.QuestionResponses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AnswerID = c.Int(nullable: false),
                        UserID = c.String(nullable: false, maxLength: 128),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RemoteCalendarLinks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RemoteCalendarHref = c.String(),
                        RemoteCalendarName = c.String(),
                        IsDisabled = c.Boolean(nullable: false),
                        LocalCalendarID = c.Int(nullable: false),
                        ProviderID = c.Int(nullable: false),
                        DateSynchronizationAttempted = c.DateTimeOffset(nullable: false, precision: 7),
                        DateSynchronized = c.DateTimeOffset(nullable: false, precision: 7),
                        LastSynchronizationResult = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._BookingRequestStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._BookingRequestAvailabilityTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._EventSyncStatusTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._EventCreateTypeTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._ParticipationStatusTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._QuestionStatusTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._AnswerStatusTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Answers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(),
                        QuestionID = c.Int(nullable: false),
                        EventID = c.Int(),
                        AnswerStatus = c.Int(),
                        UserID = c.String(maxLength: 128),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Questions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(nullable: false),
                        AnswerType = c.String(),
                        Response = c.String(),
                        QuestionStatus = c.Int(),
                        CalendarID = c.Int(),
                        NegotiationId = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._NegotiationStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Calendars",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        OwnerID = c.String(maxLength: 128),
                        NegotiationID = c.Int(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Negotiations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        NegotiationState = c.Int(),
                        BookingRequestID = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Attendees",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        EmailAddressID = c.Int(),
                        EventID = c.Int(),
                        NegotiationID = c.Int(),
                        ParticipationStatus = c.Int(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StartDate = c.DateTimeOffset(nullable: false, precision: 7),
                        EndDate = c.DateTimeOffset(nullable: false, precision: 7),
                        Location = c.String(),
                        Transparency = c.String(),
                        Class = c.String(),
                        Description = c.String(),
                        Priority = c.Int(nullable: false),
                        Sequence = c.Int(nullable: false),
                        Summary = c.String(),
                        Category = c.String(),
                        IsAllDay = c.Boolean(nullable: false),
                        ExternalGUID = c.String(),
                        EventStatus = c.Int(),
                        CreatedByID = c.String(nullable: false, maxLength: 128),
                        CalendarID = c.Int(nullable: false),
                        BookingRequestID = c.Int(),
                        CreateType = c.Int(nullable: false),
                        SyncStatus = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Emails", "ConversationId", c => c.Int());
            CreateIndex("dbo.InvitationResponses", "AttendeeId");
            CreateIndex("dbo.InvitationResponses", "Id");
            CreateIndex("dbo.BookingRequests", "Availability");
            CreateIndex("dbo.BookingRequests", "PreferredBookerID");
            CreateIndex("dbo.BookingRequests", "BookerID");
            CreateIndex("dbo.BookingRequests", "State");
            CreateIndex("dbo.BookingRequests", "CustomerID");
            CreateIndex("dbo.BookingRequests", "Id");
            CreateIndex("dbo.BookingRequestInstruction", "InstructionID");
            CreateIndex("dbo.BookingRequestInstruction", "BookingRequestID");
            CreateIndex("dbo.BookingRequestCalendar", "CalendarID");
            CreateIndex("dbo.BookingRequestCalendar", "BookingRequestID");
            CreateIndex("dbo.EventEmail", "EventID");
            CreateIndex("dbo.EventEmail", "EmailID");
            CreateIndex("dbo.NegotiationAnswerEmails", "UserID");
            CreateIndex("dbo.NegotiationAnswerEmails", "NegotiationID");
            CreateIndex("dbo.NegotiationAnswerEmails", "EmailID");
            CreateIndex("dbo.QuestionResponses", "UserID");
            CreateIndex("dbo.QuestionResponses", "AnswerID");
            CreateIndex("dbo.RemoteCalendarLinks", "ProviderID");
            CreateIndex("dbo.RemoteCalendarLinks", "LocalCalendarID");
            CreateIndex("dbo.Answers", "UserID");
            CreateIndex("dbo.Answers", "AnswerStatus");
            CreateIndex("dbo.Answers", "EventID");
            CreateIndex("dbo.Answers", "QuestionID");
            CreateIndex("dbo.Questions", "NegotiationId");
            CreateIndex("dbo.Questions", "CalendarID");
            CreateIndex("dbo.Questions", "QuestionStatus");
            CreateIndex("dbo.Calendars", "NegotiationID");
            CreateIndex("dbo.Calendars", "OwnerID");
            CreateIndex("dbo.Negotiations", "BookingRequestID");
            CreateIndex("dbo.Negotiations", "NegotiationState");
            CreateIndex("dbo.Attendees", "ParticipationStatus");
            CreateIndex("dbo.Attendees", "NegotiationID");
            CreateIndex("dbo.Attendees", "EventID");
            CreateIndex("dbo.Attendees", "EmailAddressID");
            CreateIndex("dbo.Events", "SyncStatus");
            CreateIndex("dbo.Events", "CreateType");
            CreateIndex("dbo.Events", "BookingRequestID");
            CreateIndex("dbo.Events", "CalendarID");
            CreateIndex("dbo.Events", "CreatedByID");
            CreateIndex("dbo.Events", "EventStatus");
            CreateIndex("dbo.Emails", "ConversationId");
            AddForeignKey("dbo.InvitationResponses", "AttendeeId", "dbo.Attendees", "Id");
            AddForeignKey("dbo.InvitationResponses", "Id", "dbo.Emails", "Id");
            AddForeignKey("dbo.BookingRequests", "Availability", "dbo._BookingRequestAvailabilityTemplate", "Id");
            AddForeignKey("dbo.BookingRequests", "PreferredBookerID", "dbo.Users", "Id");
            AddForeignKey("dbo.BookingRequests", "BookerID", "dbo.Users", "Id");
            AddForeignKey("dbo.BookingRequests", "State", "dbo._BookingRequestStateTemplate", "Id", cascadeDelete: true);
            AddForeignKey("dbo.BookingRequests", "CustomerID", "dbo.Users", "Id");
            AddForeignKey("dbo.BookingRequests", "Id", "dbo.Emails", "Id");
            AddForeignKey("dbo.NegotiationAnswerEmails", "UserID", "dbo.Users", "Id");
            AddForeignKey("dbo.NegotiationAnswerEmails", "NegotiationID", "dbo.Negotiations", "Id", cascadeDelete: true);
            AddForeignKey("dbo.NegotiationAnswerEmails", "EmailID", "dbo.Emails", "Id", cascadeDelete: true);
            AddForeignKey("dbo.QuestionResponses", "UserID", "dbo.Users", "Id");
            AddForeignKey("dbo.QuestionResponses", "AnswerID", "dbo.Answers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.RemoteCalendarLinks", "ProviderID", "dbo.RemoteCalendarProviders", "Id", cascadeDelete: true);
            AddForeignKey("dbo.RemoteCalendarLinks", "LocalCalendarID", "dbo.Calendars", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Calendars", "OwnerID", "dbo.Users", "Id");
            AddForeignKey("dbo.Negotiations", "BookingRequestID", "dbo.BookingRequests", "Id");
            AddForeignKey("dbo.BookingRequestInstruction", "InstructionID", "dbo.Instructions", "Id", cascadeDelete: true);
            AddForeignKey("dbo.BookingRequestInstruction", "BookingRequestID", "dbo.BookingRequests", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Emails", "ConversationId", "dbo.BookingRequests", "Id");
            AddForeignKey("dbo.BookingRequestCalendar", "CalendarID", "dbo.Calendars", "Id", cascadeDelete: true);
            AddForeignKey("dbo.BookingRequestCalendar", "BookingRequestID", "dbo.BookingRequests", "Id", cascadeDelete: true);
            AddForeignKey("dbo.EventEmail", "EventID", "dbo.Events", "Id", cascadeDelete: true);
            AddForeignKey("dbo.EventEmail", "EmailID", "dbo.Emails", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Events", "SyncStatus", "dbo._EventSyncStatusTemplate", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Events", "EventStatus", "dbo._EventStatusTemplate", "Id");
            AddForeignKey("dbo.Events", "CreateType", "dbo._EventCreateTypeTemplate", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Events", "CreatedByID", "dbo.Users", "Id");
            AddForeignKey("dbo.Events", "BookingRequestID", "dbo.BookingRequests", "Id");
            AddForeignKey("dbo.Attendees", "EventID", "dbo.Events", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Attendees", "ParticipationStatus", "dbo._ParticipationStatusTemplate", "Id");
            AddForeignKey("dbo.Questions", "NegotiationId", "dbo.Negotiations", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Questions", "QuestionStatus", "dbo._QuestionStatusTemplate", "Id");
            AddForeignKey("dbo.Questions", "CalendarID", "dbo.Calendars", "Id");
            AddForeignKey("dbo.Answers", "QuestionID", "dbo.Questions", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Answers", "UserID", "dbo.Users", "Id");
            AddForeignKey("dbo.Answers", "EventID", "dbo.Events", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Answers", "AnswerStatus", "dbo._AnswerStatusTemplate", "Id");
            AddForeignKey("dbo.Negotiations", "NegotiationState", "dbo._NegotiationStateTemplate", "Id");
            AddForeignKey("dbo.Calendars", "NegotiationID", "dbo.Negotiations", "Id");
            AddForeignKey("dbo.Events", "CalendarID", "dbo.Calendars", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Attendees", "NegotiationID", "dbo.Negotiations", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Attendees", "EmailAddressID", "dbo.EmailAddresses", "Id");
        }
    }
}
