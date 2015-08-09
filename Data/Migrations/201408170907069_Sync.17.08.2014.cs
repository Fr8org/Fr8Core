namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Sync17082014 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Concepts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RequestId = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Emails", t => t.RequestId, cascadeDelete: true)
                .Index(t => t.RequestId);
            
            DropColumn("dbo.Answers", "ObjectsType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Answers", "ObjectsType", c => c.String());
            DropForeignKey("dbo.Concepts", "RequestId", "dbo.Emails");
            DropIndex("dbo.Concepts", new[] { "RequestId" });
            DropTable("dbo.Concepts");
        }
    }
}
