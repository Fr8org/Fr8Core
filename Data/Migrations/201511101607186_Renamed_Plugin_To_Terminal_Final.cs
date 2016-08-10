namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Renamed_Plugin_To_Terminal_Final : System.Data.Entity.Migrations.DbMigration
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

            RenameColumn("dbo.Plugins", "PluginStatus", "TerminalStatus");
            RenameColumn("dbo.AuthorizationTokens", "PluginID", "TerminalID");
            RenameColumn("dbo.ActivityTemplate", "PluginId", "TerminalId");
            RenameColumn("dbo.Subscriptions", "PluginId", "TerminalId");
           
            RenameTable(name: "dbo.Plugins", newName: "Terminals");
            AddForeignKey("dbo.Terminals", "TerminalStatus", "dbo._TerminalStatusTemplate", "Id");

            CreateIndex("dbo.Subscriptions", "TerminalId");
            CreateIndex("dbo.ActivityTemplate", "TerminalId");
            CreateIndex("dbo.AuthorizationTokens", "TerminalID");
            CreateIndex("dbo.Terminals", "TerminalStatus");
            
            AddForeignKey("dbo.Subscriptions", "TerminalId", "dbo.Terminals", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ActivityTemplate", "TerminalId", "dbo.Terminals", "Id");
            AddForeignKey("dbo.AuthorizationTokens", "TerminalID", "dbo.Terminals", "Id");
        }
        
        public override void Down()
        {

            DropForeignKey("dbo.AuthorizationTokens", "TerminalID", "dbo.Terminals");
            DropForeignKey("dbo.ActivityTemplate", "TerminalId", "dbo.Terminals");
            DropForeignKey("dbo.Subscriptions", "TerminalId", "dbo.Terminals");
            DropForeignKey("dbo.Terminals", "TerminalStatus", "dbo._TerminalStatusTemplate");

            DropIndex("dbo.AuthorizationTokens", new[] { "TerminalID" });
            DropIndex("dbo.ActivityTemplate", new[] { "TerminalId" });
            DropIndex("dbo.Terminals", new[] { "TerminalStatus" });
            DropIndex("dbo.Subscriptions", new[] { "TerminalId" });
            
            RenameTable(name: "dbo.Terminals", newName: "Plugins");

            RenameColumn("dbo.Plugins", "TerminalStatus", "PluginStatus");
            RenameColumn("dbo.AuthorizationTokens", "TerminalID", "PluginID");
            RenameColumn("dbo.ActivityTemplate",  "TerminalId", "PluginId");
            RenameColumn("dbo.Subscriptions", "TerminalId", "PluginId");


            CreateIndex("dbo.AuthorizationTokens", "PluginID");
            CreateIndex("dbo.ActivityTemplate", "PluginID");
            CreateIndex("dbo.Plugins", "PluginStatus");
            CreateIndex("dbo.Subscriptions", "PluginId");

            RenameTable(name: "dbo._TerminalStatusTemplate", newName: "_PluginStatusTemplate");

            AddForeignKey("dbo.AuthorizationTokens", "PluginID", "dbo.Plugins", "Id");
            AddForeignKey("dbo.ActivityTemplate", "PluginID", "dbo.Plugins", "Id");
            AddForeignKey("dbo.Subscriptions", "PluginId", "dbo.Plugins", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Plugins", "PluginStatus", "dbo._PluginStatusTemplate", "Id", cascadeDelete: true);
        }
    }
}
