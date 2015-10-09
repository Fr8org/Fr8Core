namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UnifiedNestingMigrationFixed : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProcessNodeTemplates", "ParentTemplateId", "dbo.ProcessTemplates");
            DropForeignKey("dbo.Processes", "ProcessTemplateId", "dbo.ProcessTemplates");
            DropForeignKey("dbo.Criteria", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.ProcessNodes", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.Actions", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates");
            DropIndex("dbo.ProcessNodeTemplates", new[] { "ParentTemplateId" });
            DropPrimaryKey("dbo.ProcessNodeTemplates");
            DropPrimaryKey("dbo.ProcessTemplates");
            AlterColumn("dbo.ProcessNodeTemplates", "Id", c => c.Int(nullable: false));
            AlterColumn("dbo.ProcessTemplates", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.ProcessNodeTemplates", "Id");
            AddPrimaryKey("dbo.ProcessTemplates", "Id");
            CreateIndex("dbo.ProcessTemplates", "Id");
            CreateIndex("dbo.ProcessNodeTemplates", "Id");
            AddForeignKey("dbo.ProcessTemplates", "Id", "dbo.ActivityDOes", "Id");
            AddForeignKey("dbo.ProcessNodeTemplates", "Id", "dbo.ActivityDOes", "Id");
            AddForeignKey("dbo.Processes", "ProcessTemplateId", "dbo.ProcessTemplates", "Id");
            AddForeignKey("dbo.Criteria", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates", "Id");
            AddForeignKey("dbo.ProcessNodes", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates", "Id");
            AddForeignKey("dbo.Actions", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates", "Id");
            DropColumn("dbo.ProcessNodeTemplates", "ParentTemplateId");
            DropColumn("dbo.ProcessNodeTemplates", "LastUpdated");
            DropColumn("dbo.ProcessNodeTemplates", "CreateDate");
            DropColumn("dbo.ProcessTemplates", "ProcessNodeTemplateOrdering");
            DropColumn("dbo.ProcessTemplates", "LastUpdated");
            DropColumn("dbo.ProcessTemplates", "CreateDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProcessTemplates", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.ProcessTemplates", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.ProcessTemplates", "ProcessNodeTemplateOrdering", c => c.String());
            AddColumn("dbo.ProcessNodeTemplates", "CreateDate", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.ProcessNodeTemplates", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.ProcessNodeTemplates", "ParentTemplateId", c => c.Int(nullable: false));
            DropForeignKey("dbo.Actions", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.ProcessNodes", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.Criteria", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.Processes", "ProcessTemplateId", "dbo.ProcessTemplates");
            DropForeignKey("dbo.ProcessNodeTemplates", "Id", "dbo.ActivityDOes");
            DropForeignKey("dbo.ProcessTemplates", "Id", "dbo.ActivityDOes");
            DropIndex("dbo.ProcessNodeTemplates", new[] { "Id" });
            DropIndex("dbo.ProcessTemplates", new[] { "Id" });
            DropPrimaryKey("dbo.ProcessTemplates");
            DropPrimaryKey("dbo.ProcessNodeTemplates");
            AlterColumn("dbo.ProcessTemplates", "Id", c => c.Int(nullable: false, identity: true));
            AlterColumn("dbo.ProcessNodeTemplates", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.ProcessTemplates", "Id");
            AddPrimaryKey("dbo.ProcessNodeTemplates", "Id");
            CreateIndex("dbo.ProcessNodeTemplates", "ParentTemplateId");
            AddForeignKey("dbo.Actions", "ProcessNodeTemplateDO_Id", "dbo.ProcessNodeTemplates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ProcessNodes", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Criteria", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Processes", "ProcessTemplateId", "dbo.ProcessTemplates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ProcessNodeTemplates", "ParentTemplateId", "dbo.ProcessTemplates", "Id", cascadeDelete: true);
        }
    }
}
