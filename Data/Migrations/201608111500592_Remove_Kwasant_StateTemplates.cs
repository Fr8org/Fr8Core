namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Remove_Kwasant_StateTemplates : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Emails", "EmailStatus", "dbo._EmailStatusTemplate");
            DropForeignKey("dbo.ExpectedResponses", "Status", "dbo._ExpectedResponseStatusTemplate");
            DropForeignKey("dbo.Invitations", "Id", "dbo.Emails");
            DropForeignKey("dbo.Invitations", "InvitationType", "dbo._InvitationTypeTemplate");
            DropForeignKey("dbo.Invitations", "ConfirmationStatus", "dbo._ConfirmationStatusTemplate");
            DropIndex("dbo.Emails", new[] { "EmailStatus" });
            DropIndex("dbo.ExpectedResponses", new[] { "Status" });
            DropIndex("dbo.Invitations", new[] { "Id" });
            DropIndex("dbo.Invitations", new[] { "InvitationType" });
            DropIndex("dbo.Invitations", new[] { "ConfirmationStatus" });
            DropColumn("dbo.ExpectedResponses", "Status");
            DropTable("dbo._EmailStatusTemplate");
            DropTable("dbo._ConfirmationStatusTemplate");
            DropTable("dbo._InvitationTypeTemplate");
            DropTable("dbo._ExpectedResponseStatusTemplate");
            DropTable("dbo.Invitations");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Invitations",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        InvitationType = c.Int(),
                        ConfirmationStatus = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._ExpectedResponseStatusTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._InvitationTypeTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._ConfirmationStatusTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._EmailStatusTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ExpectedResponses", "Status", c => c.Int(nullable: false));
            CreateIndex("dbo.Invitations", "ConfirmationStatus");
            CreateIndex("dbo.Invitations", "InvitationType");
            CreateIndex("dbo.Invitations", "Id");
            CreateIndex("dbo.ExpectedResponses", "Status");
            CreateIndex("dbo.Emails", "EmailStatus");
            AddForeignKey("dbo.Invitations", "ConfirmationStatus", "dbo._ConfirmationStatusTemplate", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Invitations", "InvitationType", "dbo._InvitationTypeTemplate", "Id");
            AddForeignKey("dbo.Invitations", "Id", "dbo.Emails", "Id");
            AddForeignKey("dbo.ExpectedResponses", "Status", "dbo._ExpectedResponseStatusTemplate", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Emails", "EmailStatus", "dbo._EmailStatusTemplate", "Id");
        }
    }
}
