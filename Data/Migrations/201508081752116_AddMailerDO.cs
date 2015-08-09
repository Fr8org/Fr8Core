namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMailerDO : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Mailers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Handler = c.String(),
                        TemplateName = c.String(),
                        TemplateDescription = c.String(),
                        EmailID = c.Int(nullable: false),
                        MergeData = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Emails", t => t.EmailID, cascadeDelete: true)
                .Index(t => t.EmailID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Mailers", "EmailID", "dbo.Emails");
            DropIndex("dbo.Mailers", new[] { "EmailID" });
            DropTable("dbo.Mailers");
        }
    }
}
