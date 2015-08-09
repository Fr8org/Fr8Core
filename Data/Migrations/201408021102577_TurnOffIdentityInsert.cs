using System;

namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class TurnOffIdentityInsert : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Negotiations", "BookingRequestID", c => c.Int());
            CreateIndex("dbo.Negotiations", "BookingRequestID");
            AddForeignKey("dbo.Negotiations", "BookingRequestID", "dbo.BookingRequests", "Id");       

            DropForeignKey("dbo.Emails", "EmailStatusID", "dbo.EmailStatusRows");
            DropForeignKey("dbo.Recipients", "EmailParticipantTypeID", "dbo.EmailParticipantTypeRows");
            DropForeignKey("dbo.Events", "CreateTypeID", "dbo.EventCreateTypes");
            DropForeignKey("dbo.Events", "EventStatusID", "dbo.EventStatuses");
            DropForeignKey("dbo.Events", "SyncStatusID", "dbo.EventSyncStatus");
            DropForeignKey("dbo.ClarificationRequests", "ClarificationRequestStateID", "dbo.ClarificationRequestStates");
            DropForeignKey("dbo.Negotiations", "NegotiationStateID", "dbo.NegotiationStateRows");
            DropForeignKey("dbo.Questions", "QuestionStatusID", "dbo.QuestionStatusRows");
            DropForeignKey("dbo.BookingRequests", "BookingRequestStateID", "dbo.BookingRequestStates");
            DropForeignKey("dbo.RemoteCalendarProviders", "AuthTypeID", "dbo.ServiceAuthorizationTypes");
            DropForeignKey("dbo.TrackingStatuses", "TrackingStatusID", "dbo.TrackingStatusRows");
            DropForeignKey("dbo.TrackingStatuses", "TrackingTypeID", "dbo.TrackingTypeRows");
            DropForeignKey("dbo.Answers", "AnswerStatusID", "dbo.AnswerStatusRows");
            Sql(@"ALTER TABLE [dbo].[ClarificationRequests] DROP CONSTRAINT [FK_dbo.ClarificationRequests_dbo.BookingRequestStates_CRState]");
            Sql(@"ALTER TABLE [dbo].[BookingRequests] DROP CONSTRAINT [FK_dbo.BookingRequests_dbo.BookingRequestStatuses_BookingRequestStatusID]");

            //Ridiculous that EF makes us do this, but it doesn't handle turning off identity insert on columns properly. We need to do it manually

            ChangeTableToNonIdentity("EmailStatusRows");
            ChangeTableToNonIdentity("EmailStatusRows");
            ChangeTableToNonIdentity("EmailParticipantTypeRows");
            ChangeTableToNonIdentity("EventCreateTypes");
            ChangeTableToNonIdentity("EventStatuses");
            ChangeTableToNonIdentity("EventSyncStatus");
            ChangeTableToNonIdentity("ClarificationRequestStates");
            ChangeTableToNonIdentity("NegotiationStateRows");
            ChangeTableToNonIdentity("QuestionStatusRows");
            ChangeTableToNonIdentity("BookingRequestStates");
            ChangeTableToNonIdentity("ServiceAuthorizationTypes");
            ChangeTableToNonIdentity("TrackingStatusRows");
            ChangeTableToNonIdentity("TrackingTypeRows");
            ChangeTableToNonIdentity("AnswerStatusRows");

            AddForeignKey("dbo.Emails", "EmailStatusID", "dbo.EmailStatusRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Recipients", "EmailParticipantTypeID", "dbo.EmailParticipantTypeRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Events", "CreateTypeID", "dbo.EventCreateTypes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Events", "EventStatusID", "dbo.EventStatuses", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Events", "SyncStatusID", "dbo.EventSyncStatus", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ClarificationRequests", "ClarificationRequestStateID", "dbo.ClarificationRequestStates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Negotiations", "NegotiationStateID", "dbo.NegotiationStateRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Questions", "QuestionStatusID", "dbo.QuestionStatusRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.BookingRequests", "BookingRequestStateID", "dbo.BookingRequestStates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.RemoteCalendarProviders", "AuthTypeID", "dbo.ServiceAuthorizationTypes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TrackingStatuses", "TrackingStatusID", "dbo.TrackingStatusRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TrackingStatuses", "TrackingTypeID", "dbo.TrackingTypeRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Answers", "AnswerStatusID", "dbo.AnswerStatusRows", "Id", cascadeDelete: true);
        }

        private void ChangeTableToNonIdentity(String tableName)
        {
            var tempName = tableName + "_temp";
            var sql = String.Format(@"

BEGIN TRANSACTION

EXECUTE sp_rename N'[PK_dbo.{0}]', N'[PK_{0}_old]', 'OBJECT'

CREATE TABLE [dbo].[{1}]
(
[Id] [int] NOT NULL,
[Name] VARCHAR(MAX) NOT NULL
) ON [PRIMARY] 

ALTER TABLE [dbo].[{1}] ADD CONSTRAINT [PK_dbo.{0}] PRIMARY KEY CLUSTERED  ([Id])

INSERT INTO [dbo].[{1}] (Id, Name)
SELECT Id, Name FROM dbo.{0}

DROP TABLE dbo.{0}

EXEC sp_rename N'[dbo].[{1}]', N'{0}';

COMMIT TRANSACTION
", tableName, tempName);
            Sql(sql);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Answers", "AnswerStatusID", "dbo.AnswerStatusRows");
            DropForeignKey("dbo.TrackingStatuses", "TrackingTypeID", "dbo.TrackingTypeRows");
            DropForeignKey("dbo.TrackingStatuses", "TrackingStatusID", "dbo.TrackingStatusRows");
            DropForeignKey("dbo.RemoteCalendarProviders", "AuthTypeID", "dbo.ServiceAuthorizationTypes");
            DropForeignKey("dbo.BookingRequests", "BookingRequestStateID", "dbo.BookingRequestStates");
            DropForeignKey("dbo.Questions", "QuestionStatusID", "dbo.QuestionStatusRows");
            DropForeignKey("dbo.Negotiations", "NegotiationStateID", "dbo.NegotiationStateRows");
            DropForeignKey("dbo.ClarificationRequests", "ClarificationRequestStateID", "dbo.ClarificationRequestStates");
            DropForeignKey("dbo.Events", "SyncStatusID", "dbo.EventSyncStatus");
            DropForeignKey("dbo.Events", "EventStatusID", "dbo.EventStatuses");
            DropForeignKey("dbo.Events", "CreateTypeID", "dbo.EventCreateTypes");
            DropForeignKey("dbo.Recipients", "EmailParticipantTypeID", "dbo.EmailParticipantTypeRows");
            DropForeignKey("dbo.Emails", "EmailStatusID", "dbo.EmailStatusRows");
            DropPrimaryKey("dbo.AnswerStatusRows");
            DropPrimaryKey("dbo.TrackingTypeRows");
            DropPrimaryKey("dbo.TrackingStatusRows");
            DropPrimaryKey("dbo.ServiceAuthorizationTypes");
            DropPrimaryKey("dbo.BookingRequestStates");
            DropPrimaryKey("dbo.QuestionStatusRows");
            DropPrimaryKey("dbo.NegotiationStateRows");
            DropPrimaryKey("dbo.ClarificationRequestStates");
            DropPrimaryKey("dbo.EventSyncStatus");
            DropPrimaryKey("dbo.EventStatuses");
            DropPrimaryKey("dbo.EventCreateTypes");
            DropPrimaryKey("dbo.EmailParticipantTypeRows");
            DropPrimaryKey("dbo.EmailStatusRows");
            AlterColumn("dbo.AnswerStatusRows", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.TrackingTypeRows", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.TrackingStatusRows", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.ServiceAuthorizationTypes", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.BookingRequestStates", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.QuestionStatusRows", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.NegotiationStateRows", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.ClarificationRequestStates", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.EventSyncStatus", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.EventStatuses", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.EventCreateTypes", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.EmailParticipantTypeRows", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.EmailStatusRows", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.AnswerStatusRows", "Id");
            AddPrimaryKey("dbo.TrackingTypeRows", "Id");
            AddPrimaryKey("dbo.TrackingStatusRows", "Id");
            AddPrimaryKey("dbo.ServiceAuthorizationTypes", "Id");
            AddPrimaryKey("dbo.BookingRequestStates", "Id");
            AddPrimaryKey("dbo.QuestionStatusRows", "Id");
            AddPrimaryKey("dbo.NegotiationStateRows", "Id");
            AddPrimaryKey("dbo.ClarificationRequestStates", "Id");
            AddPrimaryKey("dbo.EventSyncStatus", "Id");
            AddPrimaryKey("dbo.EventStatuses", "Id");
            AddPrimaryKey("dbo.EventCreateTypes", "Id");
            AddPrimaryKey("dbo.EmailParticipantTypeRows", "Id");
            AddPrimaryKey("dbo.EmailStatusRows", "Id");
            AddForeignKey("dbo.Answers", "AnswerStatusID", "dbo.AnswerStatusRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TrackingStatuses", "TrackingTypeID", "dbo.TrackingTypeRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TrackingStatuses", "TrackingStatusID", "dbo.TrackingStatusRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.RemoteCalendarProviders", "AuthTypeID", "dbo.ServiceAuthorizationTypes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.BookingRequests", "BookingRequestStateID", "dbo.BookingRequestStates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Questions", "QuestionStatusID", "dbo.QuestionStatusRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Negotiations", "NegotiationStateID", "dbo.NegotiationStateRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ClarificationRequests", "ClarificationRequestStateID", "dbo.ClarificationRequestStates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Events", "SyncStatusID", "dbo.EventSyncStatus", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Events", "EventStatusID", "dbo.EventStatuses", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Events", "CreateTypeID", "dbo.EventCreateTypes", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Recipients", "EmailParticipantTypeID", "dbo.EmailParticipantTypeRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Emails", "EmailStatusID", "dbo.EmailStatusRows", "Id", cascadeDelete: true);
        }
    }
}
