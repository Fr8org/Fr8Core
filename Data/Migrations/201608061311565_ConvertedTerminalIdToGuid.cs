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
            DropPrimaryKey("dbo.Terminals");
            AlterColumn("dbo.Subscriptions", "TerminalId", c => c.Guid(nullable: false));
            AlterColumn("dbo.Terminals", "Id", c => c.Guid(nullable: false));
            AlterColumn("dbo.ActivityTemplate", "TerminalId", c => c.Guid(nullable: false));
            AlterColumn("dbo.AuthorizationTokens", "TerminalID", c => c.Guid(nullable: false));
            AlterColumn("dbo.TerminalSubscription", "TerminalId", c => c.Guid(nullable: false));
            AddPrimaryKey("dbo.Terminals", "Id");
            CreateIndex("dbo.Subscriptions", "TerminalId");
            CreateIndex("dbo.ActivityTemplate", "TerminalId");
            CreateIndex("dbo.AuthorizationTokens", "TerminalID");
            CreateIndex("dbo.TerminalSubscription", "TerminalId");
            AddForeignKey("dbo.Subscriptions", "TerminalId", "dbo.Terminals", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ActivityTemplate", "TerminalId", "dbo.Terminals", "Id");
            AddForeignKey("dbo.AuthorizationTokens", "TerminalID", "dbo.Terminals", "Id");
            AddForeignKey("dbo.TerminalSubscription", "TerminalId", "dbo.Terminals", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TerminalSubscription", "TerminalId", "dbo.Terminals");
            DropForeignKey("dbo.AuthorizationTokens", "TerminalID", "dbo.Terminals");
            DropForeignKey("dbo.ActivityTemplate", "TerminalId", "dbo.Terminals");
            DropForeignKey("dbo.Subscriptions", "TerminalId", "dbo.Terminals");
            DropIndex("dbo.TerminalSubscription", new[] { "TerminalId" });
            DropIndex("dbo.AuthorizationTokens", new[] { "TerminalID" });
            DropIndex("dbo.ActivityTemplate", new[] { "TerminalId" });
            DropIndex("dbo.Subscriptions", new[] { "TerminalId" });
            DropPrimaryKey("dbo.Terminals");
            AlterColumn("dbo.TerminalSubscription", "TerminalId", c => c.Int(nullable: false));
            AlterColumn("dbo.AuthorizationTokens", "TerminalID", c => c.Int(nullable: false));
            AlterColumn("dbo.ActivityTemplate", "TerminalId", c => c.Int(nullable: false));
            AlterColumn("dbo.Terminals", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.Subscriptions", "TerminalId", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.Terminals", "Id");
            CreateIndex("dbo.TerminalSubscription", "TerminalId");
            CreateIndex("dbo.AuthorizationTokens", "TerminalID");
            CreateIndex("dbo.ActivityTemplate", "TerminalId");
            CreateIndex("dbo.Subscriptions", "TerminalId");
            AddForeignKey("dbo.TerminalSubscription", "TerminalId", "dbo.Terminals", "Id", cascadeDelete: true);
            AddForeignKey("dbo.AuthorizationTokens", "TerminalID", "dbo.Terminals", "Id");
            AddForeignKey("dbo.ActivityTemplate", "TerminalId", "dbo.Terminals", "Id");
            AddForeignKey("dbo.Subscriptions", "TerminalId", "dbo.Terminals", "Id", cascadeDelete: true);
        }
    }
}
