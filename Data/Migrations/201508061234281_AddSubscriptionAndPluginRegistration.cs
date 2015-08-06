namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSubscriptionAndPluginRegistration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Subscriptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountId = c.String(maxLength: 128),
                        PluginRegistrationId = c.Int(nullable: false),
                        AccessLevel = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._AccessLevelTemplate", t => t.AccessLevel, cascadeDelete: true)
                .ForeignKey("dbo.PluginRegistrations", t => t.PluginRegistrationId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.AccountId)
                .Index(t => t.AccountId)
                .Index(t => t.PluginRegistrationId)
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
                "dbo.PluginRegistrations",
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
            DropForeignKey("dbo.Subscriptions", "AccountId", "dbo.Users");
            DropForeignKey("dbo.Subscriptions", "PluginRegistrationId", "dbo.PluginRegistrations");
            DropForeignKey("dbo.PluginRegistrations", "PluginStatus", "dbo._PluginStatusTemplate");
            DropForeignKey("dbo.Subscriptions", "AccessLevel", "dbo._AccessLevelTemplate");
            DropIndex("dbo.PluginRegistrations", new[] { "PluginStatus" });
            DropIndex("dbo.Subscriptions", new[] { "AccessLevel" });
            DropIndex("dbo.Subscriptions", new[] { "PluginRegistrationId" });
            DropIndex("dbo.Subscriptions", new[] { "AccountId" });
            DropTable("dbo._PluginStatusTemplate");
            DropTable("dbo.PluginRegistrations");
            DropTable("dbo._AccessLevelTemplate");
            DropTable("dbo.Subscriptions");
        }
    }
}
