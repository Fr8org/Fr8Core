namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAuthenticationManagement : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo._AuthorizationTokenStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ActionTemplate", "AuthenticationType", c => c.String());
            AddColumn("dbo.ActionTemplate", "PluginID", c => c.Int(nullable: false));
            AddColumn("dbo.AuthorizationTokens", "PluginID", c => c.Int(nullable: false));
            AddColumn("dbo.AuthorizationTokens", "AuthorizationTokenState", c => c.Int());
            CreateIndex("dbo.ActionTemplate", "PluginID");
            CreateIndex("dbo.AuthorizationTokens", "PluginID");
            CreateIndex("dbo.AuthorizationTokens", "AuthorizationTokenState");
            AddForeignKey("dbo.ActionTemplate", "PluginID", "dbo.Plugins", "Id");
            AddForeignKey("dbo.AuthorizationTokens", "AuthorizationTokenState", "dbo._AuthorizationTokenStateTemplate", "Id");
            AddForeignKey("dbo.AuthorizationTokens", "PluginID", "dbo.Plugins", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AuthorizationTokens", "PluginID", "dbo.Plugins");
            DropForeignKey("dbo.AuthorizationTokens", "AuthorizationTokenState", "dbo._AuthorizationTokenStateTemplate");
            DropForeignKey("dbo.ActionTemplate", "PluginID", "dbo.Plugins");
            DropIndex("dbo.AuthorizationTokens", new[] { "AuthorizationTokenState" });
            DropIndex("dbo.AuthorizationTokens", new[] { "PluginID" });
            DropIndex("dbo.ActionTemplate", new[] { "PluginID" });
            DropColumn("dbo.AuthorizationTokens", "AuthorizationTokenState");
            DropColumn("dbo.AuthorizationTokens", "PluginID");
            DropColumn("dbo.ActionTemplate", "PluginID");
            DropColumn("dbo.ActionTemplate", "AuthenticationType");
            DropTable("dbo._AuthorizationTokenStateTemplate");
        }
    }
}
