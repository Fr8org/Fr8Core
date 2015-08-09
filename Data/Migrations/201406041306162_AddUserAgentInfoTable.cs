namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddUserAgentInfoTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserAgentInfos",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserID = c.String(maxLength: 128),
                        RequestingURL = c.String(),
                        DeviceFamily = c.String(),
                        DeviceIsSpider = c.Boolean(nullable: false),
                        OSFamily = c.String(),
                        OSMajor = c.String(),
                        OSMinor = c.String(),
                        OSPatch = c.String(),
                        OSPatchMinor = c.String(),
                        AgentFamily = c.String(),
                        AgentMajor = c.String(),
                        AgentMinor = c.String(),
                        AgentPatch = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserAgentInfos", "UserID", "dbo.Users");
            DropIndex("dbo.UserAgentInfos", new[] { "UserID" });
            DropTable("dbo.UserAgentInfos");
        }
    }
}
