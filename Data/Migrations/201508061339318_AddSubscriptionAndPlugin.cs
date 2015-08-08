namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSubscriptionAndPlugin : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Profiles", name: "UserID", newName: "DockyardAccountID");
            RenameIndex(table: "dbo.Profiles", name: "IX_UserID", newName: "IX_DockyardAccountID");
            CreateTable(
                "dbo.Subscriptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DockyardAccountId = c.String(maxLength: 128),
                        PluginId = c.Int(nullable: false),
                        AccessLevel = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._AccessLevelTemplate", t => t.AccessLevel, cascadeDelete: true)
                .ForeignKey("dbo.Plugins", t => t.PluginId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.DockyardAccountId)
                .Index(t => t.DockyardAccountId)
                .Index(t => t.PluginId)
                .Index(t => t.AccessLevel);
            
            CreateTable(
                "dbo._AccessLevelTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Plugins",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        PluginStatus = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._PluginStatusTemplate", t => t.PluginStatus, cascadeDelete: true)
                .Index(t => t.PluginStatus);
            
            CreateTable(
                "dbo._PluginStatusTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Subscriptions", "DockyardAccountId", "dbo.Users");
            DropForeignKey("dbo.Subscriptions", "PluginId", "dbo.Plugins");
            DropForeignKey("dbo.Plugins", "PluginStatus", "dbo._PluginStatusTemplate");
            DropForeignKey("dbo.Subscriptions", "AccessLevel", "dbo._AccessLevelTemplate");
            DropIndex("dbo.Plugins", new[] { "PluginStatus" });
            DropIndex("dbo.Subscriptions", new[] { "AccessLevel" });
            DropIndex("dbo.Subscriptions", new[] { "PluginId" });
            DropIndex("dbo.Subscriptions", new[] { "DockyardAccountId" });
            DropTable("dbo._PluginStatusTemplate");
            DropTable("dbo.Plugins");
            DropTable("dbo._AccessLevelTemplate");
            DropTable("dbo.Subscriptions");
            RenameIndex(table: "dbo.Profiles", name: "IX_DockyardAccountID", newName: "IX_UserID");
            RenameColumn(table: "dbo.Profiles", name: "DockyardAccountID", newName: "UserID");
        }
    }
}
