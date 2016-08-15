namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Authorization_V2_Migration : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            // DropForeignKey("dbo.ActivityTemplate", "AuthenticationType", "dbo._AuthenticationTypeTemplate");
            // DropIndex("dbo.ActivityTemplate", new[] { "AuthenticationType" });
            // AddColumn("dbo.Terminals", "AuthenticationType", c => c.Int(nullable: false));
            // AddColumn("dbo.Actions", "AuthorizationTokenId", c => c.Guid());
            // AddColumn("dbo.ActivityTemplate", "NeedsAuthentication", c => c.Boolean(nullable: false));
            // AddColumn("dbo.AuthorizationTokens", "IsMain", c => c.Boolean(nullable: false));
            // CreateIndex("dbo.Terminals", "AuthenticationType");
            // CreateIndex("dbo.Actions", "AuthorizationTokenId");
            // AddForeignKey("dbo.Terminals", "AuthenticationType", "dbo._AuthenticationTypeTemplate", "Id", cascadeDelete: true);
            // AddForeignKey("dbo.Actions", "AuthorizationTokenId", "dbo.AuthorizationTokens", "Id");
            // DropColumn("dbo.ActivityTemplate", "AuthenticationType");

            AddColumn("dbo.Actions", "AuthorizationTokenId", c => c.Guid());
            AddColumn("dbo.AuthorizationTokens", "IsMain", c => c.Boolean(nullable: true));

            Sql("UPDATE [dbo].[AuthorizationTokens] SET [IsMain] = 0");
            AlterColumn("dbo.AuthorizationTokens", "IsMain", c => c.Boolean(nullable: false));

            CreateIndex("dbo.Actions", "AuthorizationTokenId");
            AddForeignKey("dbo.Actions", "AuthorizationTokenId", "dbo.AuthorizationTokens", "Id");

            AddColumn("dbo.ActivityTemplate", "NeedsAuthentication", c => c.Boolean(nullable: true));
            Sql("UPDATE [dbo].[ActivityTemplate] SET [NeedsAuthentication] = 0");
            Sql("UPDATE [dbo].[ActivityTemplate] SET [NeedsAuthentication] = 1 WHERE [AuthenticationType] > 1");
            AlterColumn("dbo.ActivityTemplate", "NeedsAuthentication", c => c.Boolean(nullable: false));

            AddColumn("dbo.Terminals", "AuthenticationType", c => c.Int(nullable: true));
            Sql("UPDATE [dbo].[Terminals] SET [AuthenticationType] = 1");
            Sql(
                @"UPDATE [t]
                SET [t].[AuthenticationType] = [r].[AuthenticationType]
                FROM [dbo].[Terminals] [t]
                INNER JOIN (
                    SELECT DISTINCT
                        [a].[AuthenticationType],
                        [a].[TerminalId]
                    FROM [dbo].[ActivityTemplate] [a]
                    WHERE [a].[AuthenticationType] > 1
                ) [r]
                    ON [r].[TerminalId] = [t].[Id]");
            AlterColumn("dbo.Terminals", "AuthenticationType", c => c.Int(nullable: false));
            CreateIndex("dbo.Terminals", "AuthenticationType");
            AddForeignKey("dbo.Terminals", "AuthenticationType", "dbo._AuthenticationTypeTemplate", "Id", cascadeDelete: true);

            DropForeignKey("dbo.ActivityTemplate", "AuthenticationType", "dbo._AuthenticationTypeTemplate");
            DropIndex("dbo.ActivityTemplate", new[] { "AuthenticationType" });
            DropColumn("dbo.ActivityTemplate", "AuthenticationType");
        }

        public override void Down()
        {
            // AddColumn("dbo.ActivityTemplate", "AuthenticationType", c => c.Int(nullable: false));
            // DropForeignKey("dbo.Actions", "AuthorizationTokenId", "dbo.AuthorizationTokens");
            // DropForeignKey("dbo.Terminals", "AuthenticationType", "dbo._AuthenticationTypeTemplate");
            // DropIndex("dbo.Actions", new[] { "AuthorizationTokenId" });
            // DropIndex("dbo.Terminals", new[] { "AuthenticationType" });
            // DropColumn("dbo.AuthorizationTokens", "IsMain");
            // DropColumn("dbo.ActivityTemplate", "NeedsAuthentication");
            // DropColumn("dbo.Actions", "AuthorizationTokenId");
            // DropColumn("dbo.Terminals", "AuthenticationType");
            // CreateIndex("dbo.ActivityTemplate", "AuthenticationType");
            // AddForeignKey("dbo.ActivityTemplate", "AuthenticationType", "dbo._AuthenticationTypeTemplate", "Id", cascadeDelete: true);

            DropForeignKey("dbo.Actions", "AuthorizationTokenId", "dbo.AuthorizationTokens");
            DropIndex("dbo.Actions", new[] { "AuthorizationTokenId" });
            DropColumn("dbo.AuthorizationTokens", "IsMain");
            DropColumn("dbo.Actions", "AuthorizationTokenId");

            AddColumn("dbo.ActivityTemplate", "AuthenticationType", c => c.Int(nullable: true));
            Sql("UPDATE [dbo].[ActivityTemplate] SET [AuthenticationType] = 1");
            Sql(
                @"UPDATE [a]
                SET [a].[AuthenticationType] = [r].[AuthenticationType]
                FROM [dbo].[ActivityTemplate] [a]
                INNER JOIN (
                    SELECT DISTINCT
                        [a].[Id],
                        [t].[AuthenticationType]
                    FROM [dbo].[ActivityTemplate] [a]
                    INNER JOIN [dbo].[Terminals] [t]
                        ON [t].[Id] = [a].[TerminalId]
                    WHERE [a].[NeedsAuthentication] = 1
                ) [r]
                    ON [r].[Id] = [a].[Id]");
            AlterColumn("dbo.ActivityTemplate", "AuthenticationType", c => c.Int(nullable: false));
            CreateIndex("dbo.ActivityTemplate", "AuthenticationType");
            AddForeignKey("dbo.ActivityTemplate", "AuthenticationType", "dbo._AuthenticationTypeTemplate", "Id", cascadeDelete: true);

            DropForeignKey("dbo.Terminals", "AuthenticationType", "dbo._AuthenticationTypeTemplate");
            DropIndex("dbo.Terminals", new[] { "AuthenticationType" });
            DropColumn("dbo.Terminals", "AuthenticationType");

            DropColumn("dbo.ActivityTemplate", "NeedsAuthentication");
        }
    }
}
