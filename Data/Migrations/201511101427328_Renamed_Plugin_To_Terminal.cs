namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Renamed_Plugin_To_Terminal : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo._PluginStatusTemplate", newName: "_TerminalStatusTemplate");
            DropForeignKey("dbo.Plugins", "PluginStatus", "dbo._PluginStatusTemplate");
            DropForeignKey("dbo.Subscriptions", "PluginId", "dbo.Plugins");
            DropForeignKey("dbo.ActivityTemplate", "PluginID", "dbo.Plugins");
            DropForeignKey("dbo.AuthorizationTokens", "PluginID", "dbo.Plugins");
            DropIndex("dbo.Subscriptions", new[] { "PluginId" });
            DropIndex("dbo.Plugins", new[] { "PluginStatus" });
            DropIndex("dbo.ActivityTemplate", new[] { "PluginID" });
            DropIndex("dbo.AuthorizationTokens", new[] { "PluginID" });
            CreateTable(
                "dbo.Terminals",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Version = c.String(nullable: false),
                        TerminalStatus = c.Int(nullable: false),
                        Endpoint = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._TerminalStatusTemplate", t => t.TerminalStatus, cascadeDelete: true)
                .Index(t => t.TerminalStatus);
            
            AddColumn("dbo.Subscriptions", "TerminalId", c => c.Int(nullable: false));
            AddColumn("dbo.ActivityTemplate", "TerminalId", c => c.Int(nullable: false));
            AddColumn("dbo.AuthorizationTokens", "TerminalID", c => c.Int(nullable: false));
            CreateIndex("dbo.Subscriptions", "TerminalId");
            CreateIndex("dbo.ActivityTemplate", "TerminalId");
            CreateIndex("dbo.AuthorizationTokens", "TerminalID");
            AddForeignKey("dbo.Subscriptions", "TerminalId", "dbo.Terminals", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ActivityTemplate", "TerminalId", "dbo.Terminals", "Id");
            AddForeignKey("dbo.AuthorizationTokens", "TerminalID", "dbo.Terminals", "Id");
            DropColumn("dbo.Subscriptions", "PluginId");
            DropColumn("dbo.ActivityTemplate", "PluginID");
            DropColumn("dbo.AuthorizationTokens", "PluginID");
            DropTable("dbo.Plugins");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Plugins",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Version = c.String(nullable: false),
                        PluginStatus = c.Int(nullable: false),
                        Endpoint = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.AuthorizationTokens", "PluginID", c => c.Int(nullable: false));
            AddColumn("dbo.ActivityTemplate", "PluginID", c => c.Int(nullable: false));
            AddColumn("dbo.Subscriptions", "PluginId", c => c.Int(nullable: false));
            DropForeignKey("dbo.AuthorizationTokens", "TerminalID", "dbo.Terminals");
            DropForeignKey("dbo.ActivityTemplate", "TerminalId", "dbo.Terminals");
            DropForeignKey("dbo.Subscriptions", "TerminalId", "dbo.Terminals");
            DropForeignKey("dbo.Terminals", "TerminalStatus", "dbo._TerminalStatusTemplate");
            DropIndex("dbo.AuthorizationTokens", new[] { "TerminalID" });
            DropIndex("dbo.ActivityTemplate", new[] { "TerminalId" });
            DropIndex("dbo.Terminals", new[] { "TerminalStatus" });
            DropIndex("dbo.Subscriptions", new[] { "TerminalId" });
            DropColumn("dbo.AuthorizationTokens", "TerminalID");
            DropColumn("dbo.ActivityTemplate", "TerminalId");
            DropColumn("dbo.Subscriptions", "TerminalId");
            DropTable("dbo.Terminals");
            CreateIndex("dbo.AuthorizationTokens", "PluginID");
            CreateIndex("dbo.ActivityTemplate", "PluginID");
            CreateIndex("dbo.Plugins", "PluginStatus");
            CreateIndex("dbo.Subscriptions", "PluginId");
            AddForeignKey("dbo.AuthorizationTokens", "PluginID", "dbo.Plugins", "Id");
            AddForeignKey("dbo.ActivityTemplate", "PluginID", "dbo.Plugins", "Id");
            AddForeignKey("dbo.Subscriptions", "PluginId", "dbo.Plugins", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Plugins", "PluginStatus", "dbo._PluginStatusTemplate", "Id", cascadeDelete: true);
            RenameTable(name: "dbo._TerminalStatusTemplate", newName: "_PluginStatusTemplate");
        }
    }
}
