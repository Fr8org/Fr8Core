namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class OrganizeStates : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Emails", name: "EmailStatusID", newName: "EmailStatus");
            RenameIndex(table: "dbo.Emails", name: "IX_EmailStatusID", newName: "IX_EmailStatus");

            RenameColumn(table: "dbo.BookingRequests", name: "BookingRequestStateID", newName: "BookingRequestState");
            RenameIndex(table: "dbo.BookingRequests", name: "IX_BookingRequestStateID", newName: "IX_BookingRequestState");

            RenameColumn(table: "dbo.ClarificationRequests", name: "ClarificationRequestStateID", newName: "ClarificationRequestState");
            RenameIndex(table: "dbo.ClarificationRequests", name: "IX_ClarificationRequestStateID", newName: "IX_ClarificationRequestState");

            RenameColumn(table: "dbo.Events", name: "EventStatusID", newName: "EventStatus");
            RenameIndex(table: "dbo.Events", name: "IX_EventStatusID", newName: "IX_EventStatus");

            RenameColumn(table: "dbo.Events", name: "CreateTypeID", newName: "CreateType");
            RenameIndex(table: "dbo.Events", name: "IX_CreateTypeID", newName: "IX_CreateType");

            RenameColumn(table: "dbo.Events", name: "SyncStatusID", newName: "SyncStatus");
            RenameIndex(table: "dbo.Events", name: "IX_SyncStatusID", newName: "IX_SyncStatus");

            RenameColumn(table: "dbo.Recipients", name: "EmailParticipantTypeID", newName: "EmailParticipantType");
            RenameIndex(table: "dbo.Recipients", name: "IX_EmailParticipantTypeID", newName: "IX_EmailParticipantType");

            RenameColumn(table: "dbo.Negotiations", name: "NegotiationStateID", newName: "NegotiationState");
            RenameIndex(table: "dbo.Negotiations", name: "IX_NegotiationStateID", newName: "IX_NegotiationState");

            RenameColumn(table: "dbo.Questions", name: "QuestionStatusID", newName: "QuestionStatus");
            RenameIndex(table: "dbo.Questions", name: "IX_QuestionStatusID", newName: "IX_QuestionStatus");

            RenameColumn(table: "dbo.Answers", name: "AnswerStatusID", newName: "AnswerStatus");
            RenameIndex(table: "dbo.Answers", name: "IX_AnswerStatusID", newName: "IX_AnswerStatus");

            RenameColumn(table: "dbo.RemoteCalendarProviders", name: "AuthTypeID", newName: "AuthType");
            RenameIndex(table: "dbo.RemoteCalendarProviders", name: "IX_AuthTypeID", newName: "IX_AuthType");

            RenameColumn(table: "dbo.CommunicationConfigurations", name: "CommunicationTypeID", newName: "CommunicationType");
            RenameIndex(table: "dbo.CommunicationConfigurations", name: "IX_CommunicationTypeID", newName: "IX_CommunicationType");

            RenameColumn(table: "dbo.TrackingStatuses", name: "TrackingTypeID", newName: "TrackingType");
            RenameIndex(table: "dbo.TrackingStatuses", name: "IX_TrackingTypeID", newName: "IX_TrackingType");

            RenameColumn(table: "dbo.TrackingStatuses", name: "TrackingStatusID", newName: "TrackingStatus");
            RenameIndex(table: "dbo.TrackingStatuses", name: "IX_TrackingStatusID", newName: "IX_TrackingStatus");

            RenameTable(name: "dbo.EmailStatusRows", newName: "_EmailStatusTemplate");
            RenameTable(name: "dbo.EmailParticipantTypeRows", newName: "_EmailParticipantTypeTemplate");
            RenameTable(name: "dbo.EventCreateTypes", newName: "_EventCreateTypeTemplate");
            RenameTable(name: "dbo.EventStatuses", newName: "_EventStatusTemplate");
            RenameTable(name: "dbo.EventSyncStatus", newName: "_EventSyncStatusTemplate");
            RenameTable(name: "dbo.ClarificationRequestStates", newName: "_ClarificationRequestStateTemplate");
            RenameTable(name: "dbo.NegotiationStateRows", newName: "_NegotiationStateTemplate");
            RenameTable(name: "dbo.AnswerStatusRows", newName: "_AnswerStatusTemplate");
            RenameTable(name: "dbo.QuestionStatusRows", newName: "_QuestionStatusTemplate");
            RenameTable(name: "dbo.BookingRequestStates", newName: "_BookingRequestStateTemplate");
            RenameTable(name: "dbo.ServiceAuthorizationTypes", newName: "_ServiceAuthorizationTypeTemplate");
            RenameTable(name: "dbo.CommunicationTypeRows", newName: "_CommunicationTypeTemplate");
            RenameTable(name: "dbo.TrackingStatusRows", newName: "_TrackingStatusTemplate");
            RenameTable(name: "dbo.TrackingTypeRows", newName: "_TrackingTypeTemplate");
        }
        
        public override void Down()
        {
            RenameColumn(table: "dbo.Emails", name: "EmailStatus", newName: "EmailStatusID");
            RenameIndex(table: "dbo.Emails", name: "IX_EmailStatus", newName: "IX_EmailStatusID");

            RenameColumn(table: "dbo.BookingRequests", name: "BookingRequestState", newName: "BookingRequestStateID");
            RenameIndex(table: "dbo.BookingRequests", name: "IX_BookingRequestState", newName: "IX_BookingRequestStateID");

            RenameColumn(table: "dbo.ClarificationRequests", name: "ClarificationRequestState", newName: "ClarificationRequestStateID");
            RenameIndex(table: "dbo.ClarificationRequests", name: "IX_ClarificationRequestState", newName: "IX_ClarificationRequestStateID");

            RenameColumn(table: "dbo.Events", name: "EventStatus", newName: "EventStatusID");
            RenameIndex(table: "dbo.Events", name: "IX_EventStatus", newName: "IX_EventStatusID");

            RenameColumn(table: "dbo.Events", name: "CreateType", newName: "CreateTypeID");
            RenameIndex(table: "dbo.Events", name: "IX_CreateType", newName: "IX_CreateTypeID");

            RenameColumn(table: "dbo.Events", name: "SyncStatus", newName: "SyncStatusID");
            RenameIndex(table: "dbo.Events", name: "IX_SyncStatus", newName: "IX_SyncStatusID");

            RenameColumn(table: "dbo.Recipients", name: "EmailParticipantType", newName: "EmailParticipantTypeID");
            RenameIndex(table: "dbo.Recipients", name: "IX_EmailParticipantType", newName: "IX_EmailParticipantTypeID");

            RenameColumn(table: "dbo.Negotiations", name: "NegotiationState", newName: "NegotiationStateID");
            RenameIndex(table: "dbo.Negotiations", name: "IX_NegotiationState", newName: "IX_NegotiationStateID");

            RenameColumn(table: "dbo.Questions", name: "QuestionStatus", newName: "QuestionStatusID");
            RenameIndex(table: "dbo.Questions", name: "IX_QuestionStatus", newName: "IX_QuestionStatusID");

            RenameColumn(table: "dbo.Answers", name: "AnswerStatus", newName: "AnswerStatusID");
            RenameIndex(table: "dbo.Answers", name: "IX_AnswerStatus", newName: "IX_AnswerStatusID");

            RenameColumn(table: "dbo.RemoteCalendarProviders", name: "AuthType", newName: "AuthTypeID");
            RenameIndex(table: "dbo.RemoteCalendarProviders", name: "IX_AuthType", newName: "IX_AuthTypeID");

            RenameColumn(table: "dbo.CommunicationConfigurations", name: "CommunicationType", newName: "CommunicationTypeID");
            RenameIndex(table: "dbo.CommunicationConfigurations", name: "IX_CommunicationType", newName: "IX_CommunicationTypeID");

            RenameColumn(table: "dbo.TrackingStatuses", name: "TrackingType", newName: "TrackingTypeID");
            RenameIndex(table: "dbo.TrackingStatuses", name: "IX_TrackingType", newName: "IX_TrackingTypeID");

            RenameColumn(table: "dbo.TrackingStatuses", name: "TrackingStatus", newName: "TrackingStatusID");
            RenameIndex(table: "dbo.TrackingStatuses", name: "IX_TrackingStatus", newName: "IX_TrackingStatusID");

            RenameTable(name: "dbo._EmailStatusTemplate", newName: "EmailStatusRows");
            RenameTable(name: "dbo._EmailParticipantTypeTemplate", newName: "EmailParticipantTypeRows");
            RenameTable(name: "dbo._EventCreateTypeTemplate", newName: "EventCreateTypes");
            RenameTable(name: "dbo._EventStatusTemplate", newName: "EventStatuses");
            RenameTable(name: "dbo._EventSyncStatusTemplate", newName: "EventSyncStatus");
            RenameTable(name: "dbo._ClarificationRequestStateTemplate", newName: "ClarificationRequestStates");
            RenameTable(name: "dbo._NegotiationStateTemplate", newName: "NegotiationStateRows");
            RenameTable(name: "dbo._AnswerStatusTemplate", newName: "AnswerStatusRows");
            RenameTable(name: "dbo._QuestionStatusTemplate", newName: "QuestionStatusRows");
            RenameTable(name: "dbo._BookingRequestStateTemplate", newName: "BookingRequestStates");
            RenameTable(name: "dbo._ServiceAuthorizationTypeTemplate", newName: "ServiceAuthorizationTypes");
            RenameTable(name: "dbo._CommunicationTypeTemplate", newName: "CommunicationTypeRows");
            RenameTable(name: "dbo._TrackingStatusTemplate", newName: "TrackingStatusRows");
            RenameTable(name: "dbo._TrackingTypeTemplate", newName: "TrackingTypeRows");
        }
    }
}
