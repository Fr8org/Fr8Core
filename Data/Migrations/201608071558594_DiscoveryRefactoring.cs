namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DiscoveryRefactoring : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TerminalRegistration", "UserId", "dbo.Users");
            DropIndex("dbo.TerminalRegistration", new[] { "UserId" });
            RenameColumn(table: "dbo.Terminals", name: "UserDO_Id", newName: "UserId");
            RenameIndex(table: "dbo.Terminals", name: "IX_UserDO_Id", newName: "IX_UserId");
            CreateTable(
                "dbo._ParticipationStateTemplate",
                c => new
                {
                    Id = c.Int(nullable: false),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);
            Sql("INSERT INTO dbo._ParticipationStateTemplate(Id, Name) VALUES (1, 'Approved')");
            AddColumn("dbo.Terminals", "IsFr8OwnTerminal", c => c.Boolean(nullable: false));
            AddColumn("dbo.Terminals", "DevUrl", c => c.String());
            AddColumn("dbo.Terminals", "ProdUrl", c => c.String());
            AddColumn("dbo.Terminals", "ParticipationState", c => c.Int(nullable: false, defaultValue: 1));
            CreateIndex("dbo.Terminals", "ParticipationState");
            AddForeignKey("dbo.Terminals", "ParticipationState", "dbo._ParticipationStateTemplate", "Id", cascadeDelete: true);
            DropTable("dbo.TerminalRegistration");
            Sql("UPDATE Terminals SET DevUrl = Endpoint WHERE [Endpoint] LIKE '%localhost%'");

            AlterColumn("dbo.Terminals", "Name", c => c.String());
            AlterColumn("dbo.Terminals", "Version", c => c.String());
            AlterColumn("dbo.Terminals", "Label", c => c.String());
            AlterColumn("dbo.Terminals", "AuthenticationType", c => c.Int());
            AlterColumn("dbo.Terminals", "Endpoint", c => c.String(nullable: false));
        }

        public override void Down()
        {
            CreateTable(
                "dbo.TerminalRegistration",
                c => new
                {
                    Endpoint = c.String(nullable: false, maxLength: 128),
                    UserId = c.String(maxLength: 128),
                    IsFr8OwnTerminal = c.Boolean(nullable: false),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                })
                .PrimaryKey(t => t.Endpoint);

            DropForeignKey("dbo.Terminals", "ParticipationState", "dbo._ParticipationStateTemplate");
            DropIndex("dbo.Terminals", new[] { "ParticipationState" });
            DropColumn("dbo.Terminals", "ParticipationState");
            DropColumn("dbo.Terminals", "ProdUrl");
            DropColumn("dbo.Terminals", "DevUrl");
            DropColumn("dbo.Terminals", "IsFr8OwnTerminal");
            DropTable("dbo._ParticipationStateTemplate");
            RenameIndex(table: "dbo.Terminals", name: "IX_UserId", newName: "IX_UserDO_Id");
            RenameColumn(table: "dbo.Terminals", name: "UserId", newName: "UserDO_Id");
            CreateIndex("dbo.TerminalRegistration", "UserId");
            AddForeignKey("dbo.TerminalRegistration", "UserId", "dbo.Users", "Id");

            Sql("UPDATE Terminals SET Label = Name WHERE Label IS NULL");
            Sql("UPDATE Terminals SET Label = 'No label' WHERE Label IS NULL");
            Sql("UPDATE Terminals SET Version = 'No version' WHERE Version IS NULL");
            Sql("UPDATE Terminals SET Name = 'No name' WHERE Name IS NULL");
            AlterColumn("dbo.Terminals", "Label", c => c.String(nullable: false));
            AlterColumn("dbo.Terminals", "Version", c => c.String(nullable: false));
            AlterColumn("dbo.Terminals", "Name", c => c.String(nullable: false));
            DropIndex("dbo.Terminals", "IX_AuthenticationType");
            AlterColumn("dbo.Terminals", "AuthenticationType", c => c.Int(nullable: false));
            CreateIndex("dbo.Terminals", "AuthenticationType", false, "IX_AuthenticationType");
            AlterColumn("dbo.Terminals", "Endpoint", c => c.String());
        }
    }
}
