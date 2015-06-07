namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddEnvelopeDO : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Envelopes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Handler = c.String(),
                        TemplateName = c.String(),
                        MergeData = c.String(),
                        EmailID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Emails", t => t.EmailID)
                .Index(t => t.EmailID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Envelopes", "EmailID", "dbo.Emails");
            DropIndex("dbo.Envelopes", new[] { "EmailID" });
            DropTable("dbo.Envelopes");
        }
    }
}
