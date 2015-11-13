namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Renamed_PluginId_To_TerminalId : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.ActivityTemplate", name: "PluginID", newName: "TerminalID");
            RenameIndex(table: "dbo.ActivityTemplate", name: "IX_PluginID", newName: "IX_TerminalID");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.ActivityTemplate", name: "IX_TerminalID", newName: "IX_PluginID");
            RenameColumn(table: "dbo.ActivityTemplate", name: "TerminalID", newName: "PluginID");
        }
    }
}
