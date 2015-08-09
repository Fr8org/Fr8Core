namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Sync02122014 : DbMigration
    {
        public override void Up()
        {
            //DropForeignKey("dbo.Emails", "ConfirmationStatus", "dbo._ConfirmationStatusTemplate");
            //DropForeignKey("dbo.Emails", "InvitationType", "dbo._InvitationTypeTemplate");
            //DropIndex("dbo.Emails", new[] { "InvitationType" });
            //DropIndex("dbo.Emails", new[] { "ConfirmationStatus" });
            //CreateTable(
            //    "dbo.Invitations",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false),
            //            InvitationType = c.Int(),
            //            ConfirmationStatus = c.Int(nullable: false),
            //        })
            //    .PrimaryKey(t => t.Id)
            //    .ForeignKey("dbo.Emails", t => t.Id)
            //    .ForeignKey("dbo._InvitationTypeTemplate", t => t.InvitationType)
            //    .ForeignKey("dbo._ConfirmationStatusTemplate", t => t.ConfirmationStatus, cascadeDelete: true)
            //    .Index(t => t.Id)
            //    .Index(t => t.InvitationType)
            //    .Index(t => t.ConfirmationStatus);
            
            //DropColumn("dbo.Emails", "InvitationType");
            //DropColumn("dbo.Emails", "ConfirmationStatus");
            //DropColumn("dbo.Emails", "Discriminator");
        }
        
        public override void Down()
        {
            //AddColumn("dbo.Emails", "Discriminator", c => c.String(maxLength: 128));
            //AddColumn("dbo.Emails", "ConfirmationStatus", c => c.Int());
            //AddColumn("dbo.Emails", "InvitationType", c => c.Int());
            //DropForeignKey("dbo.Invitations", "ConfirmationStatus", "dbo._ConfirmationStatusTemplate");
            //DropForeignKey("dbo.Invitations", "InvitationType", "dbo._InvitationTypeTemplate");
            //DropForeignKey("dbo.Invitations", "Id", "dbo.Emails");
            //DropIndex("dbo.Invitations", new[] { "ConfirmationStatus" });
            //DropIndex("dbo.Invitations", new[] { "InvitationType" });
            //DropIndex("dbo.Invitations", new[] { "Id" });
            //DropTable("dbo.Invitations");
            //CreateIndex("dbo.Emails", "ConfirmationStatus");
            //CreateIndex("dbo.Emails", "InvitationType");
            //AddForeignKey("dbo.Emails", "InvitationType", "dbo._InvitationTypeTemplate", "Id");
            //AddForeignKey("dbo.Emails", "ConfirmationStatus", "dbo._ConfirmationStatusTemplate", "Id", cascadeDelete: true);
        }
    }
}
