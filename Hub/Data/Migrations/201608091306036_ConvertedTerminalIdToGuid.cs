namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class ConvertedTerminalIdToGuid : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Subscriptions", "TerminalId", "dbo.Terminals");
            DropForeignKey("dbo.ActivityTemplate", "TerminalId", "dbo.Terminals");
            DropForeignKey("dbo.AuthorizationTokens", "TerminalID", "dbo.Terminals");
            DropForeignKey("dbo.TerminalSubscription", "TerminalId", "dbo.Terminals");

            DropIndex("dbo.Subscriptions", new[] { "TerminalId" });
            DropIndex("dbo.ActivityTemplate", new[] { "TerminalId" });
            DropIndex("dbo.AuthorizationTokens", new[] { "TerminalID" });
            DropIndex("dbo.TerminalSubscription", new[] { "TerminalId" });

            RenameTable("dbo.Terminals", "OldTerminals");

            CreateTable(
                "dbo.Terminals",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    Secret = c.String(),
                    Name = c.String(),
                    Version = c.String(),
                    Label = c.String(),
                    TerminalStatus = c.Int(nullable: false),
                    Endpoint = c.String(nullable: false),
                    Description = c.String(),
                    AuthenticationType = c.Int(),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    UserId = c.String(maxLength: 128),
                    IsFr8OwnTerminal = c.Boolean(nullable: false),
                    DevUrl = c.String(),
                    ProdUrl = c.String(),
                    ParticipationState = c.Int(nullable: false, defaultValue: 1),
                    OldId = c.Int(nullable: false)
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._AuthenticationTypeTemplate", t => t.AuthenticationType, true, "FK_dbo.Terminals_dbo._AuthenticationTypeTemplate_AuthenticationType_")
                .ForeignKey("dbo._TerminalStatusTemplate", t => t.TerminalStatus, true, "FK_dbo.Terminals_dbo._TerminalStatusTemplate_TerminalStatus_")
                .ForeignKey("dbo.Users", t => t.UserId, false, "FK_dbo.Terminals_dbo._Users_UserId_")
                .ForeignKey("dbo._ParticipationStateTemplate", t => t.ParticipationState, true, "FK_dbo.Terminals_dbo._ParticipationStateTemplate_ParticipationState_")
                .Index(t => t.TerminalStatus)
                .Index(t => t.AuthenticationType)
                .Index(t => t.ParticipationState)
                .Index(t => t.UserId);

            Sql(
                @"INSERT INTO [dbo].[Terminals] (
                    [Id],
                    [Secret],
                    [Name],
                    [Version],
                    [Label],
                    [TerminalStatus],
                    [Endpoint],
                    [Description],
                    [AuthenticationType],
                    [LastUpdated],
                    [CreateDate],
                    [UserId],
                    [IsFr8OwnTerminal],
                    [DevUrl],
                    [ProdUrl],
                    [ParticipationState],
                    [OldId])
                SELECT
                    newid() as [Id],
                    [ot].[Secret],
                    [ot].[Name],
                    [ot].[Version],
                    [ot].[Label],
                    [ot].[TerminalStatus],
                    [ot].[Endpoint],
                    [ot].[Description],
                    [ot].[AuthenticationType],
                    [ot].[LastUpdated],
                    [ot].[CreateDate],
                    [ot].[UserId],
                    [ot].[IsFr8OwnTerminal],
                    [ot].[DevUrl],
                    [ot].[ProdUrl],
                    [ot].[ParticipationState],
                    [ot].[Id] as [OldId]
                FROM [dbo].[OldTerminals] AS [ot]"
            );
            /* dbo.Subscriptions Modifications */
            RenameColumn("dbo.Subscriptions", "TerminalId", "OldTerminalId");
            AddColumn("dbo.Subscriptions", "TerminalId", c => c.Guid(nullable: true));
            Sql("UPDATE [SB] SET [TerminalId] = [T].[Id] FROM [dbo].[Subscriptions] AS [SB] INNER JOIN [dbo].[Terminals] [T] ON [T].[OldId] = [SB].[OldTerminalId]");
            AlterColumn("dbo.Subscriptions", "TerminalId", c => c.Guid(nullable: false));
            CreateIndex("dbo.Subscriptions", "TerminalId");
            AddForeignKey("dbo.Subscriptions", "TerminalId", "dbo.Terminals", "Id");
            DropColumn("dbo.Subscriptions", "OldTerminalId");
            /* dbo.ActivityTemplate Modifications */
            RenameColumn("dbo.ActivityTemplate", "TerminalId", "OldTerminalId");
            AddColumn("dbo.ActivityTemplate", "TerminalId", c => c.Guid(nullable: true));
            Sql("UPDATE [AT] SET [TerminalId] = [T].[Id] FROM [dbo].[ActivityTemplate] AS [AT] INNER JOIN [dbo].[Terminals] [T] ON [T].[OldId] = [AT].[OldTerminalId]");
            AlterColumn("dbo.ActivityTemplate", "TerminalId", c => c.Guid(nullable: false));
            CreateIndex("dbo.ActivityTemplate", "TerminalId");
            AddForeignKey("dbo.ActivityTemplate", "TerminalId", "dbo.Terminals", "Id");
            DropColumn("dbo.ActivityTemplate", "OldTerminalId");
            /* dbo.AuthorizationTokens Modifications */
            RenameColumn("dbo.AuthorizationTokens", "TerminalID", "OldTerminalId");
            AddColumn("dbo.AuthorizationTokens", "TerminalID", c => c.Guid(nullable: true));
            Sql("UPDATE [AT] SET [TerminalId] = [T].[Id] FROM [dbo].[AuthorizationTokens] AS [AT] INNER JOIN [dbo].[Terminals] [T] ON [T].[OldId] = [AT].[OldTerminalId]");
            AlterColumn("dbo.AuthorizationTokens", "TerminalID", c => c.Guid(nullable: false));
            CreateIndex("dbo.AuthorizationTokens", "TerminalID");
            AddForeignKey("dbo.AuthorizationTokens", "TerminalID", "dbo.Terminals", "Id");
            DropColumn("dbo.AuthorizationTokens", "OldTerminalId");
            /* dbo.TerminalSubscription Modifications */
            RenameColumn("dbo.TerminalSubscription", "TerminalId", "OldTerminalId");
            AddColumn("dbo.TerminalSubscription", "TerminalId", c => c.Guid(nullable: true));
            Sql("UPDATE [AT] SET [TerminalId] = [T].[Id] FROM [dbo].[TerminalSubscription] AS [AT] INNER JOIN [dbo].[Terminals] [T] ON [T].[OldId] = [AT].[OldTerminalId]");
            AlterColumn("dbo.TerminalSubscription", "TerminalId", c => c.Guid(nullable: false));
            CreateIndex("dbo.TerminalSubscription", "TerminalId");
            AddForeignKey("dbo.TerminalSubscription", "TerminalId", "dbo.Terminals", "Id");
            DropColumn("dbo.TerminalSubscription", "OldTerminalId");

            //Modify ObjectRolePermission ObjectId for terminals
            Sql(@"UPDATE o SET o.ObjectId = t.Id
                     FROM Terminals t
                     INNER JOIN ObjectRolePermissions o ON
                     o.ObjectId = t.OldId
                     WHERE o.Type = 'TerminalDO'");

            //Remove leftovers
            DropColumn("dbo.Terminals", "OldId");
            DropTable("dbo.OldTerminals");
        }

        public override void Down()
        {

        }
    }
}
