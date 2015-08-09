namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProcessFlow_Migration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExternalEventRegistrations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventType = c.Int(nullable: false),
                        ProcessTemplateId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProcessTemplates", t => t.ProcessTemplateId)
                .Index(t => t.ProcessTemplateId);
            
            CreateTable(
                "dbo.DocuSignEvents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExternalEventType = c.Int(nullable: false),
                        RecipientId = c.String(),
                        EnvelopeId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ProcessTemplates", "ProcessNodeTemplateOrdering", c => c.String());
            AddColumn("dbo.ProcessTemplates", "StartingProcessNodeTemplate", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExternalEventRegistrations", "ProcessTemplateId", "dbo.ProcessTemplates");
            DropIndex("dbo.ExternalEventRegistrations", new[] { "ProcessTemplateId" });
            DropColumn("dbo.ProcessTemplates", "StartingProcessNodeTemplate");
            DropColumn("dbo.ProcessTemplates", "ProcessNodeTemplateOrdering");
            DropTable("dbo.DocuSignEvents");
            DropTable("dbo.ExternalEventRegistrations");
        }
    }
}
