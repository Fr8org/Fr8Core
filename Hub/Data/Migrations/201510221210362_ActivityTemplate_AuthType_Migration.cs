using Fr8.Infrastructure.Data.States;

namespace Data.Migrations
{
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
            
            SeedConstants<AuthenticationType>("dbo._AuthenticationTypeTemplate");
            Sql(string.Format("UPDATE dbo.ActivityTemplate SET AuthenticationType = {0} WHERE AuthenticationType IS NULL", AuthenticationType.None));

            Sql(string.Format("UPDATE dbo.ActivityTemplate SET AuthenticationType = {0} WHERE PluginID IN (SELECT Id FROM dbo.Plugins WHERE Name = '{1}')", AuthenticationType.Internal, "terminalDocuSign"));
            Sql(string.Format("UPDATE dbo.ActivityTemplate SET AuthenticationType = {0} WHERE PluginID IN (SELECT Id FROM dbo.Plugins WHERE Name = '{1}')", AuthenticationType.External, "terminalSlack"));

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
