namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DocuSignTemplateSubscription_Migration : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.DocuSignTemplateSubscriptions", "ProcessTemplateId", "dbo.ProcessTemplates");
            DropIndex("dbo.DocuSignTemplateSubscriptions", new[] { "ProcessTemplateId" });
            RenameColumn(table: "dbo.ExternalEventRegistrations", name: "ProcessTemplateId", newName: "ExternalProcessTemplateId");
            RenameColumn(table: "dbo.DocuSignTemplateSubscriptions", name: "ProcessTemplateId", newName: "DocuSignProcessTemplateId");
            RenameIndex(table: "dbo.ExternalEventRegistrations", name: "IX_ProcessTemplateId", newName: "IX_ExternalProcessTemplateId");
            DropPrimaryKey("dbo.DocuSignTemplateSubscriptions");
            AddColumn("dbo.ExternalEventRegistrations", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.ExternalEventRegistrations", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7));
            AlterColumn("dbo.DocuSignTemplateSubscriptions", "Id", c => c.Int(nullable: false));
            AlterColumn("dbo.DocuSignTemplateSubscriptions", "DocuSignProcessTemplateId", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.DocuSignTemplateSubscriptions", "Id");
            CreateIndex("dbo.DocuSignTemplateSubscriptions", "Id");
            CreateIndex("dbo.DocuSignTemplateSubscriptions", "DocuSignProcessTemplateId");
            AddForeignKey("dbo.DocuSignTemplateSubscriptions", "Id", "dbo.ExternalEventRegistrations", "Id");
            AddForeignKey("dbo.DocuSignTemplateSubscriptions", "DocuSignProcessTemplateId", "dbo.ProcessTemplates", "Id", cascadeDelete: true);
            DropColumn("dbo.DocuSignTemplateSubscriptions", "LastUpdated");
            DropColumn("dbo.DocuSignTemplateSubscriptions", "CreateDate");
            DropColumn("dbo.ExternalEventRegistrations", "DocuSignTemplateId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ExternalEventRegistrations", "DocuSignTemplateId", c => c.String());
            AddColumn("dbo.DocuSignTemplateSubscriptions", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.DocuSignTemplateSubscriptions", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            DropForeignKey("dbo.DocuSignTemplateSubscriptions", "DocuSignProcessTemplateId", "dbo.ProcessTemplates");
            DropForeignKey("dbo.DocuSignTemplateSubscriptions", "Id", "dbo.ExternalEventRegistrations");
            DropIndex("dbo.DocuSignTemplateSubscriptions", new[] { "DocuSignProcessTemplateId" });
            DropIndex("dbo.DocuSignTemplateSubscriptions", new[] { "Id" });
            DropPrimaryKey("dbo.DocuSignTemplateSubscriptions");
            AlterColumn("dbo.DocuSignTemplateSubscriptions", "DocuSignProcessTemplateId", c => c.Int());
            AlterColumn("dbo.DocuSignTemplateSubscriptions", "Id", c => c.Int(nullable: false, identity: true));
            DropColumn("dbo.ExternalEventRegistrations", "CreateDate");
            DropColumn("dbo.ExternalEventRegistrations", "LastUpdated");
            AddPrimaryKey("dbo.DocuSignTemplateSubscriptions", "Id");
            RenameIndex(table: "dbo.ExternalEventRegistrations", name: "IX_ExternalProcessTemplateId", newName: "IX_ProcessTemplateId");
            RenameColumn(table: "dbo.DocuSignTemplateSubscriptions", name: "DocuSignProcessTemplateId", newName: "ProcessTemplateId");
            RenameColumn(table: "dbo.ExternalEventRegistrations", name: "ExternalProcessTemplateId", newName: "ProcessTemplateId");
            CreateIndex("dbo.DocuSignTemplateSubscriptions", "ProcessTemplateId");
            AddForeignKey("dbo.DocuSignTemplateSubscriptions", "ProcessTemplateId", "dbo.ProcessTemplates", "Id");
        }
    }
}
