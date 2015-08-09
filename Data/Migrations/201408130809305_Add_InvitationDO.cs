namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_InvitationDO : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo._ConfirmationStatusTemplate",
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
            
            AddColumn("dbo.Emails", "InvitationType", c => c.Int());
            AddColumn("dbo.Emails", "ConfirmationStatus", c => c.Int());
            AddColumn("dbo.Emails", "Discriminator", c => c.String(maxLength: 128));
            CreateIndex("dbo.Emails", "InvitationType");
            CreateIndex("dbo.Emails", "ConfirmationStatus");
            AddForeignKey("dbo.Emails", "ConfirmationStatus", "dbo._ConfirmationStatusTemplate", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Emails", "InvitationType", "dbo._InvitationTypeTemplate", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Emails", "InvitationType", "dbo._InvitationTypeTemplate");
            DropForeignKey("dbo.Emails", "ConfirmationStatus", "dbo._ConfirmationStatusTemplate");
            DropIndex("dbo.Emails", new[] { "ConfirmationStatus" });
            DropIndex("dbo.Emails", new[] { "InvitationType" });
            DropColumn("dbo.Emails", "Discriminator");
            DropColumn("dbo.Emails", "ConfirmationStatus");
            DropColumn("dbo.Emails", "InvitationType");
            DropTable("dbo._InvitationTypeTemplate");
            DropTable("dbo._ConfirmationStatusTemplate");
        }
    }
}
