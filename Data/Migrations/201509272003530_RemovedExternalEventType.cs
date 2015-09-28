namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedExternalEventType : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ExternalEventSubscriptions", "ExternalEvent", "dbo._ExternalEventTypeTemplate");
            DropForeignKey("dbo.ExternalEventSubscriptions", "ExternalProcessTemplateId", "dbo.ProcessTemplates");
            DropForeignKey("dbo.ActionLists", "TriggerEventID", "dbo._ExternalEventTypeTemplate");
            DropForeignKey("dbo.DocuSignTemplateSubscriptions", "Id", "dbo.ExternalEventSubscriptions");
            DropForeignKey("dbo.DocuSignTemplateSubscriptions", "DocuSignProcessTemplateId", "dbo.ProcessTemplates");
            DropIndex("dbo.ExternalEventSubscriptions", new[] { "ExternalEvent" });
            DropIndex("dbo.ExternalEventSubscriptions", new[] { "ExternalProcessTemplateId" });
            DropIndex("dbo.ActionLists", new[] { "TriggerEventID" });
            DropIndex("dbo.DocuSignTemplateSubscriptions", new[] { "Id" });
            DropIndex("dbo.DocuSignTemplateSubscriptions", new[] { "DocuSignProcessTemplateId" });
            DropColumn("dbo.ActionLists", "TriggerEventID");
            DropTable("dbo.ExternalEventSubscriptions");
            DropTable("dbo._ExternalEventTypeTemplate");
            DropTable("dbo.DocuSignTemplateSubscriptions");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.DocuSignTemplateSubscriptions",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        DocuSignProcessTemplateId = c.Int(nullable: false),
                        DocuSignTemplateId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._ExternalEventTypeTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ExternalEventSubscriptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExternalEvent = c.Int(),
                        ExternalProcessTemplateId = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ActionLists", "TriggerEventID", c => c.Int());
            CreateIndex("dbo.DocuSignTemplateSubscriptions", "DocuSignProcessTemplateId");
            CreateIndex("dbo.DocuSignTemplateSubscriptions", "Id");
            CreateIndex("dbo.ActionLists", "TriggerEventID");
            CreateIndex("dbo.ExternalEventSubscriptions", "ExternalProcessTemplateId");
            CreateIndex("dbo.ExternalEventSubscriptions", "ExternalEvent");
            AddForeignKey("dbo.DocuSignTemplateSubscriptions", "DocuSignProcessTemplateId", "dbo.ProcessTemplates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.DocuSignTemplateSubscriptions", "Id", "dbo.ExternalEventSubscriptions", "Id");
            AddForeignKey("dbo.ActionLists", "TriggerEventID", "dbo._ExternalEventTypeTemplate", "Id");
            AddForeignKey("dbo.ExternalEventSubscriptions", "ExternalProcessTemplateId", "dbo.ProcessTemplates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ExternalEventSubscriptions", "ExternalEvent", "dbo._ExternalEventTypeTemplate", "Id");
        }
    }
}
