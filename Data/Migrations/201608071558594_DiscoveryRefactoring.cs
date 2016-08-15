namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DiscoveryRefactoring : System.Data.Entity.Migrations.DbMigration
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
            Sql("UPDATE Terminals SET DevUrl = [Endpoint] WHERE [Endpoint] LIKE '%localhost%'"); // for local env
            Sql("UPDATE Terminals SET DevUrl = [Endpoint] WHERE [Endpoint] LIKE '%dev-terminals.fr8.co%'"); // for dev env
            Sql(@"
                SET NOCOUNT ON
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:39768' WHERE [Endpoint] LIKE '%terminalAtlassian.fr8.co%'
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:46281' WHERE [Endpoint] LIKE '%terminalAzure.fr8.co%'
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:61121' WHERE [Endpoint] LIKE '%terminalBasecamp2.fr8.co%'
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:53234' WHERE [Endpoint] LIKE '%terminalDocuSign.fr8.co%'
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:19760' WHERE [Endpoint] LIKE '%terminalDropbox.fr8.co%'
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:47011' WHERE [Endpoint] LIKE '%terminalExcel.fr8.co%'
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:22666' WHERE [Endpoint] LIKE '%terminalFacebook.fr8.co%'
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:50705' WHERE [Endpoint] LIKE '%terminalFr8Core.fr8.co%'
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:30701' WHERE [Endpoint] LIKE '%terminalPapertrail.fr8.co%'
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:48317' WHERE [Endpoint] LIKE '%terminalQuickBooks.fr8.co%'
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:51234' WHERE [Endpoint] LIKE '%terminalSalesforce.fr8.co%'
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:10601' WHERE [Endpoint] LIKE '%terminalSendGrid.fr8.co%'
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:39504' WHERE [Endpoint] LIKE '%terminalSlack.fr8.co%'
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:59022' WHERE [Endpoint] LIKE '%terminalTelegram.fr8.co%'
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:30699' WHERE [Endpoint] LIKE '%terminalTwilio.fr8.co%'
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:56785' WHERE [Endpoint] LIKE '%terminalAsana.fr8.co%'
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:25923' WHERE [Endpoint] LIKE '%terminalGoogle.fr8.co%'	
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:39555' WHERE [Endpoint] LIKE '%terminalYammer.fr8.co%'
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:10109' WHERE [Endpoint] LIKE '%terminalInstagram.fr8.co%'
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:54642' WHERE [Endpoint] LIKE '%terminalBox.fr8.co%'
                UPDATE [dbo].[Terminals] SET DevUrl = 'http://localhost:    ' WHERE [Endpoint] LIKE '%terminalStatX.fr8.co%'
                SET NOCOUNT OFF
            "); // for Prod
            Sql("UPDATE Terminals SET IsFr8OwnTerminal = 1 WHERE [DevUrl] LIKE '%localhost:%' OR [ProdUrl] LIKE '%fr8.co:%'");
            AlterColumn("dbo.Terminals", "Name", c => c.String());
            AlterColumn("dbo.Terminals", "Version", c => c.String());
            AlterColumn("dbo.Terminals", "Label", c => c.String());
            AlterColumn("dbo.Terminals", "AuthenticationType", c => c.Int());
            AlterColumn("dbo.Terminals", "Endpoint", c => c.String(nullable: false));

            Sql(@"
                SET NOCOUNT ON
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalAtlassian.fr8.co' WHERE [DevUrl] LIKE '%:39768%'
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalAzure.fr8.co' WHERE [DevUrl] LIKE '%:46281%'
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalBasecamp2.fr8.co' WHERE [DevUrl] LIKE '%:61121%'
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalDocuSign.fr8.co' WHERE [DevUrl] LIKE '%:53234%'
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalDropbox.fr8.co' WHERE [DevUrl] LIKE '%:19760%'
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalExcel.fr8.co' WHERE [DevUrl] LIKE '%:47011%'
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalFacebook.fr8.co' WHERE [DevUrl] LIKE '%:22666%'
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalFr8Core.fr8.co' WHERE [DevUrl] LIKE '%:50705%'
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalPapertrail.fr8.co' WHERE [DevUrl] LIKE '%:30701%'
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalQuickBooks.fr8.co' WHERE [DevUrl] LIKE '%:48317%'
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalSalesforce.fr8.co' WHERE [DevUrl] LIKE '%:51234%'
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalSendGrid.fr8.co' WHERE [DevUrl] LIKE '%:10601%'
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalSlack.fr8.co' WHERE [DevUrl] LIKE '%:39504%'
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalTelegram.fr8.co' WHERE [DevUrl] LIKE '%:59022%'
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalTwilio.fr8.co' WHERE [DevUrl] LIKE '%:30699%'
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalAsana.fr8.co' WHERE [DevUrl] LIKE '%:56785%'
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalGoogle.fr8.co' WHERE [DevUrl] LIKE '%:25923%'	
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalYammer.fr8.co' WHERE [DevUrl] LIKE '%:39555%'
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalInstagram.fr8.co' WHERE [DevUrl] LIKE '%:10109%'
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalBox.fr8.co' WHERE [DevUrl] LIKE '%:54642%'
                UPDATE [dbo].[Terminals] SET ProdUrl = 'https://terminalStatX.fr8.co' WHERE [DevUrl] LIKE '%:48675%'
                SET NOCOUNT OFF
            ");
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
