namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConvertedTerminalIdToGuid : DbMigration
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
                    Name = c.String(nullable: false),
                    Version = c.String(nullable: false),
                    Label = c.String(nullable: false),
                    TerminalStatus = c.Int(nullable: false),
                    Endpoint = c.String(),
                    Description = c.String(),
                    AuthenticationType = c.Int(nullable: false),
                    LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    UserDO_Id = c.String(maxLength: 128),
                    OldId = c.Int(nullable: false)
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._AuthenticationTypeTemplate", t => t.AuthenticationType, cascadeDelete: true)
                .ForeignKey("dbo._TerminalStatusTemplate", t => t.TerminalStatus, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserDO_Id)
                .Index(t => t.TerminalStatus)
                .Index(t => t.AuthenticationType)
                .Index(t => t.UserDO_Id);

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
                    [UserDO_Id],
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
                    [ot].[UserDO_Id],
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

            //Remove leftovers
            DropColumn("dbo.Terminals", "OldId");
            DropTable("dbo.OldTerminals");
        }
        
        public override void Down()
        {
            
        }
    }
}
