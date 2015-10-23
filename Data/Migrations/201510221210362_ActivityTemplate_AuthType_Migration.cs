namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using Data.States;
    
    public partial class ActivityTemplate_AuthType_Migration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo._AuthenticationTypeTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AlterColumn("dbo.ActivityTemplate", "AuthenticationType", c => c.Int(nullable: false));
            CreateIndex("dbo.ActivityTemplate", "AuthenticationType");
            AddForeignKey("dbo.ActivityTemplate", "AuthenticationType", "dbo._AuthenticationTypeTemplate", "Id", cascadeDelete: true);
            DropColumn("dbo.Plugins", "RequiresAuthentication");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Plugins", "RequiresAuthentication", c => c.Boolean(nullable: false));
            DropForeignKey("dbo.ActivityTemplate", "AuthenticationType", "dbo._AuthenticationTypeTemplate");
            DropIndex("dbo.ActivityTemplate", new[] { "AuthenticationType" });
            AlterColumn("dbo.ActivityTemplate", "AuthenticationType", c => c.String());
            DropTable("dbo._AuthenticationTypeTemplate");
        }
    }
}
