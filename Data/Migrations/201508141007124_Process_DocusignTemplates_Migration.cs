namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Process_DocusignTemplates_Migration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DocuSignTemplateSubscriptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProcessTemplateId = c.Int(),
                        DocuSignTemplateId = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProcessTemplates", t => t.ProcessTemplateId)
                .Index(t => t.ProcessTemplateId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DocuSignTemplateSubscriptions", "ProcessTemplateId", "dbo.ProcessTemplates");
            DropIndex("dbo.DocuSignTemplateSubscriptions", new[] { "ProcessTemplateId" });
            DropTable("dbo.DocuSignTemplateSubscriptions");
        }
    }
}
