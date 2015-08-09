using Data.States;

namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddInvitationResponse : KwasantDbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo._ParticipationStatusTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);

            SeedConstants<ParticipationStatus>("dbo._ParticipationStatusTemplate");

            CreateTable(
                "dbo.InvitationResponses",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        AttendeeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Emails", t => t.Id)
                .ForeignKey("dbo.Attendees", t => t.AttendeeId, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.AttendeeId);
            
            AddColumn("dbo.Emails", "ReplyToID", c => c.Int());
            AddColumn("dbo.Attendees", "ParticipationStatus", c => c.Int(nullable: false));
            CreateIndex("dbo.Emails", "ReplyToID");
            CreateIndex("dbo.Attendees", "ParticipationStatus");
            AddForeignKey("dbo.Attendees", "ParticipationStatus", "dbo._ParticipationStatusTemplate", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Emails", "ReplyToID", "dbo.EmailAddresses", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.InvitationResponses", "AttendeeId", "dbo.Attendees");
            DropForeignKey("dbo.InvitationResponses", "Id", "dbo.Emails");
            DropForeignKey("dbo.Emails", "ReplyToID", "dbo.EmailAddresses");
            DropForeignKey("dbo.Attendees", "ParticipationStatus", "dbo._ParticipationStatusTemplate");
            DropIndex("dbo.InvitationResponses", new[] { "AttendeeId" });
            DropIndex("dbo.InvitationResponses", new[] { "Id" });
            DropIndex("dbo.Attendees", new[] { "ParticipationStatus" });
            DropIndex("dbo.Emails", new[] { "ReplyToID" });
            DropColumn("dbo.Attendees", "ParticipationStatus");
            DropColumn("dbo.Emails", "ReplyToID");
            DropTable("dbo.InvitationResponses");
            DropTable("dbo._ParticipationStatusTemplate");
        }
    }
}
