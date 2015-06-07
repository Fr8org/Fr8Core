using Data.States;

namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class UpdateStateHandling : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Events", "StateID", "dbo.EventStatuses");
            DropIndex("dbo.Events", new[] { "StateID" });
            RenameColumn(table: "dbo.BookingRequests", name: "BRState", newName: "BookingRequestStateID");
            RenameIndex(table: "dbo.BookingRequests", name: "IX_BRState", newName: "IX_BookingRequestStateID");
            RenameColumn(table: "dbo.ClarificationRequests", name: "CRState", newName: "ClarificationRequestStateID");
            RenameIndex(table: "dbo.ClarificationRequests", name: "IX_CRState", newName: "IX_ClarificationRequestStateID");

            CreateTable(
                "dbo.EmailStatusRows",
                c => new
                         {
                             Id = c.Int(nullable: false, identity: true),
                             Name = c.String(),
                         })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.EmailParticipantTypeRows",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.QuestionStatusRows",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CommunicationTypeRows",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TrackingStatusRows",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TrackingTypeRows",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            
            AddColumn("dbo.Emails", "EmailStatusID", c => c.Int(nullable: false));
            AddColumn("dbo.Events", "EventStatusID", c => c.Int(nullable: false));
            AddColumn("dbo.Recipients", "EmailParticipantTypeID", c => c.Int(nullable: false));
            AddColumn("dbo.Questions", "QuestionStatusID", c => c.Int(nullable: false));
            AddColumn("dbo.CommunicationConfigurations", "CommunicationTypeID", c => c.Int(nullable: false));
            AddColumn("dbo.TrackingStatuses", "TrackingTypeID", c => c.Int(nullable: false));
            AddColumn("dbo.TrackingStatuses", "TrackingStatusID", c => c.Int(nullable: false));

            RenameColumn("dbo.Negotiations", "State", "NegotiationStateID");
            CreateIndex("dbo.Negotiations", "NegotiationStateID");
            AddForeignKey("dbo.Negotiations", "NegotiationStateID", "dbo.NegotiationStateRows", cascadeDelete: true);

            RenameColumn("dbo.Answers", name: "Status", newName: "AnswerStatusID");
            AddForeignKey("dbo.Answers", "AnswerStatusID", "dbo.AnswerStatusRows", cascadeDelete: true);
            CreateIndex("dbo.Answers", "AnswerStatusID");

            Sql(@"Update dbo.Emails SET EmailStatusID = EmailStatus");
            Sql(@"Update dbo.Events SET EventStatusID = StateID");
            Sql(@"Update dbo.Recipients SET EmailParticipantTypeID = (Type)");
            Sql(@"Update dbo.Questions SET QuestionStatusID = (Status + 1)"); // +1, because it was originally 0indexed
            Sql(@"Update dbo.CommunicationConfigurations SET CommunicationTypeID = (Type + 1)"); // +1, because it was originally 0indexed
            Sql(@"Update dbo.TrackingStatuses SET TrackingTypeID = Type");
            Sql(@"Update dbo.TrackingStatuses SET TrackingStatusID = Status");


            //We need to seed all this before updating the DB
            Sql(@"
SET IDENTITY_INSERT dbo.EmailStatusRows ON;
INSERT INTO dbo.EmailStatusRows (Id, Name) 
VALUES (" + EmailState.Dispatched + @",'')
INSERT INTO dbo.EmailStatusRows (Id, Name) 
VALUES (" + EmailState.Invalid + @",'')
INSERT INTO dbo.EmailStatusRows (Id, Name) 
VALUES (" + EmailState.Processed + @",'')
INSERT INTO dbo.EmailStatusRows (Id, Name) 
VALUES (" + EmailState.Queued + @",'')
INSERT INTO dbo.EmailStatusRows (Id, Name) 
VALUES (" + EmailState.SendCriticalError + @",'')
INSERT INTO dbo.EmailStatusRows (Id, Name) 
VALUES (" + EmailState.SendRejected + @",'')
INSERT INTO dbo.EmailStatusRows (Id, Name) 
VALUES (" + EmailState.Sent + @",'')
INSERT INTO dbo.EmailStatusRows (Id, Name) 
VALUES (" + EmailState.Unprocessed + @",'')
INSERT INTO dbo.EmailStatusRows (Id, Name) 
VALUES (" + EmailState.Unstarted + @",'')
SET IDENTITY_INSERT dbo.EmailStatusRows OFF;");

            Sql(@"
SET IDENTITY_INSERT dbo.EmailParticipantTypeRows ON;
INSERT INTO dbo.EmailParticipantTypeRows (Id, Name) 
VALUES (" + EmailParticipantType.Bcc + @",'')
INSERT INTO dbo.EmailParticipantTypeRows (Id, Name) 
VALUES (" + EmailParticipantType.Cc + @",'')
INSERT INTO dbo.EmailParticipantTypeRows (Id, Name) 
VALUES (" + EmailParticipantType.To + @",'')
SET IDENTITY_INSERT dbo.EmailParticipantTypeRows OFF;");

            Sql(@"
SET IDENTITY_INSERT dbo.CommunicationTypeRows ON;
INSERT INTO dbo.CommunicationTypeRows (Id, Name) 
VALUES (" + CommunicationType.Email + @",'')
INSERT INTO dbo.CommunicationTypeRows (Id, Name) 
VALUES (" + CommunicationType.Sms + @",'')
SET IDENTITY_INSERT dbo.CommunicationTypeRows OFF;");

            Sql(@"
SET IDENTITY_INSERT dbo.TrackingStatusRows ON;
INSERT INTO dbo.TrackingStatusRows (Id, Name) 
VALUES (" + TrackingState.Completed + @",'')
INSERT INTO dbo.TrackingStatusRows (Id, Name) 
VALUES (" + TrackingState.Invalid + @",'')
INSERT INTO dbo.TrackingStatusRows (Id, Name) 
VALUES (" + TrackingState.PendingClarification + @",'')
INSERT INTO dbo.TrackingStatusRows (Id, Name) 
VALUES (" + TrackingState.Processed + @",'')
INSERT INTO dbo.TrackingStatusRows (Id, Name) 
VALUES (" + TrackingState.Unprocessed + @",'')
INSERT INTO dbo.TrackingStatusRows (Id, Name) 
VALUES (" + TrackingState.Unstarted + @",'')
SET IDENTITY_INSERT dbo.TrackingStatusRows OFF;");

            Sql(@"
SET IDENTITY_INSERT dbo.TrackingTypeRows ON;
INSERT INTO dbo.TrackingTypeRows (Id, Name) 
VALUES (" + TrackingType.BookingState + @",'')
INSERT INTO dbo.TrackingTypeRows (Id, Name) 
VALUES (" + TrackingType.TestState + @",'')
SET IDENTITY_INSERT dbo.TrackingTypeRows OFF;");
            
            CreateIndex("dbo.Emails", "EmailStatusID");
            CreateIndex("dbo.Events", "EventStatusID");
            CreateIndex("dbo.Recipients", "EmailParticipantTypeID");
            CreateIndex("dbo.Questions", "QuestionStatusID");
            CreateIndex("dbo.CommunicationConfigurations", "CommunicationTypeID");
            CreateIndex("dbo.TrackingStatuses", "TrackingTypeID");
            CreateIndex("dbo.TrackingStatuses", "TrackingStatusID");
            AddForeignKey("dbo.Emails", "EmailStatusID", "dbo.EmailStatusRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Recipients", "EmailParticipantTypeID", "dbo.EmailParticipantTypeRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Events", "EventStatusID", "dbo.EventStatuses", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Questions", "QuestionStatusID", "dbo.QuestionStatusRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.CommunicationConfigurations", "CommunicationTypeID", "dbo.CommunicationTypeRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TrackingStatuses", "TrackingStatusID", "dbo.TrackingStatusRows", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TrackingStatuses", "TrackingTypeID", "dbo.TrackingTypeRows", "Id", cascadeDelete: true);
            DropColumn("dbo.Emails", "EmailStatus");
            DropColumn("dbo.Events", "StateID");
            DropColumn("dbo.Recipients", "Type");
            DropColumn("dbo.Questions", "Status");
            DropColumn("dbo.CommunicationConfigurations", "Type");
            DropColumn("dbo.TrackingStatuses", "Type");
            DropColumn("dbo.TrackingStatuses", "Status");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TrackingStatuses", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.TrackingStatuses", "Type", c => c.Int(nullable: false));
            AddColumn("dbo.CommunicationConfigurations", "Type", c => c.Int(nullable: false));
            AddColumn("dbo.Questions", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.Recipients", "Type", c => c.Int(nullable: false));
            AddColumn("dbo.Events", "StateID", c => c.Int(nullable: false));
            AddColumn("dbo.Emails", "EmailStatus", c => c.Int(nullable: false));

            Sql(@"Update dbo.Emails SET EmailStatus = EmailStatusID");
            Sql(@"Update dbo.Events SET StateID = EventStatusID");
            Sql(@"Update dbo.Recipients SET Type = (EmailParticipantTypeID + 1)"); // +1, because we moved from 1-2-3 to 2-3-4
            Sql(@"Update dbo.Questions SET Status = (QuestionStatusID - 1)"); // -1, because it was originally 0indexed
            Sql(@"Update dbo.CommunicationConfigurations SET Type = (CommunicationTypeID - 1)"); // -1, because it was originally 0indexed
            Sql(@"Update dbo.TrackingStatuses SET Type = TrackingTypeID");
            Sql(@"Update dbo.TrackingStatuses SET Status = TrackingStatusID");

            RenameColumn("dbo.Negotiations", "NegotiationStateID", "State");
            RenameColumn("dbo.Answers", name: "AnswerStatusID", newName: "Status");
            DropForeignKey("dbo.Answers", "AnswerStatusID", "dbo.AnswerStatusRows");
            DropForeignKey("dbo.Answers", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Answers", "QuestionID", "dbo.Questions");
            DropForeignKey("dbo.Answers", "AnswerStatusID", "dbo.AnswerStatusRows");
            DropForeignKey("dbo.TrackingStatuses", "TrackingTypeID", "dbo.TrackingTypeRows");
            DropForeignKey("dbo.TrackingStatuses", "TrackingStatusID", "dbo.TrackingStatusRows");
            DropForeignKey("dbo.CommunicationConfigurations", "CommunicationTypeID", "dbo.CommunicationTypeRows");
            DropForeignKey("dbo.Questions", "QuestionStatusID", "dbo.QuestionStatusRows");
            DropForeignKey("dbo.Negotiations", "NegotiationStateID", "dbo.NegotiationStateRows");
            DropForeignKey("dbo.Events", "EventStatusID", "dbo.EventStatuses");
            DropForeignKey("dbo.Recipients", "EmailParticipantTypeID", "dbo.EmailParticipantTypeRows");
            DropForeignKey("dbo.Emails", "EmailStatusID", "dbo.EmailStatusRows");
            DropIndex("dbo.Answers", new[] { "User_Id" });
            DropIndex("dbo.Answers", new[] { "AnswerStatusID" });
            DropIndex("dbo.Answers", new[] { "QuestionID" });
            DropIndex("dbo.TrackingStatuses", new[] { "TrackingStatusID" });
            DropIndex("dbo.TrackingStatuses", new[] { "TrackingTypeID" });
            DropIndex("dbo.CommunicationConfigurations", new[] { "CommunicationTypeID" });
            DropIndex("dbo.Questions", new[] { "QuestionStatusID" });
            DropIndex("dbo.Negotiations", new[] { "NegotiationStateID" });
            DropIndex("dbo.Answers", new[] { "AnswerStatusID" });
            DropIndex("dbo.Recipients", new[] { "EmailParticipantTypeID" });
            DropIndex("dbo.Events", new[] { "EventStatusID" });
            DropIndex("dbo.Emails", new[] { "EmailStatusID" });
            DropColumn("dbo.TrackingStatuses", "TrackingStatusID");
            DropColumn("dbo.TrackingStatuses", "TrackingTypeID");
            DropColumn("dbo.CommunicationConfigurations", "CommunicationTypeID");
            DropColumn("dbo.Questions", "QuestionStatusID");
            DropColumn("dbo.Recipients", "EmailParticipantTypeID");
            DropColumn("dbo.Events", "EventStatusID");
            DropColumn("dbo.Emails", "EmailStatusID");
            DropTable("dbo.AnswerStatusRows");
            DropTable("dbo.TrackingTypeRows");
            DropTable("dbo.TrackingStatusRows");
            DropTable("dbo.CommunicationTypeRows");
            DropTable("dbo.QuestionStatusRows");
            DropTable("dbo.NegotiationStateRows");
            DropTable("dbo.EmailParticipantTypeRows");
            DropTable("dbo.EmailStatusRows");

            RenameIndex(table: "dbo.ClarificationRequests", name: "IX_ClarificationRequestStateID", newName: "IX_CRState");
            RenameColumn(table: "dbo.ClarificationRequests", name: "ClarificationRequestStateID", newName: "CRState");
            RenameIndex(table: "dbo.BookingRequests", name: "IX_BookingRequestStateID", newName: "IX_BRState");
            RenameColumn(table: "dbo.BookingRequests", name: "BookingRequestStateID", newName: "BRState");
            CreateIndex("dbo.Events", "StateID");
            AddForeignKey("dbo.Events", "StateID", "dbo.EventStatuses", "Id", cascadeDelete: true);
        }
    }
}
