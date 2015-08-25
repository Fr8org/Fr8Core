namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProcessTemplateAndProcessaddl : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Processes", "ParentProcessTemplateId", "dbo.ProcessTemplates");
            DropForeignKey("dbo.ProcessNodes", "ProcessTemplateDO_Id", "dbo.ProcessTemplates");
            DropForeignKey("dbo.ProcessNodes", "ProcessNodeTemplate_Id", "dbo.ProcessNodeTemplates");
            DropIndex("dbo.Processes", new[] { "ParentProcessTemplateId" });
            DropIndex("dbo.ProcessNodes", new[] { "ProcessNodeTemplate_Id" });
            DropIndex("dbo.ProcessNodes", new[] { "ProcessTemplateDO_Id" });
            DropIndex("dbo.Criteria", new[] { "ProcessNodeTemplateID" });
            RenameColumn(table: "dbo.Processes", name: "ProcessTemplateDO_Id", newName: "ProcessTemplateId");
            RenameColumn(table: "dbo.ProcessNodes", name: "ProcessNodeTemplate_Id", newName: "ProcessNodeTemplateId");
            RenameColumn(table: "dbo.Criteria", name: "ExecutionType", newName: "CriteriaExecutionType");
            RenameIndex(table: "dbo.Criteria", name: "IX_ExecutionType", newName: "IX_CriteriaExecutionType");
            AddColumn("dbo.ProcessTemplates", "StartingProcessNodeTemplateId", c => c.Int(nullable: false));
            AlterColumn("dbo.ProcessNodes", "ProcessNodeTemplateId", c => c.Int(nullable: false));
            CreateIndex("dbo.Processes", "ProcessTemplateId");
            CreateIndex("dbo.ProcessNodes", "ProcessNodeTemplateId");
            CreateIndex("dbo.Criteria", "ProcessNodeTemplateId");
            AddForeignKey("dbo.Processes", "ProcessTemplateId", "dbo.ProcessTemplates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ProcessNodes", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates", "Id", cascadeDelete: true);
            DropColumn("dbo.Processes", "ParentProcessTemplateId");
            DropColumn("dbo.ProcessTemplates", "StartingProcessNodeTemplate");
            DropColumn("dbo.ProcessNodes", "ProcessTemplateDO_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProcessNodes", "ProcessTemplateDO_Id", c => c.Int());
            AddColumn("dbo.ProcessTemplates", "StartingProcessNodeTemplate", c => c.Int(nullable: false));
            AddColumn("dbo.Processes", "ParentProcessTemplateId", c => c.Int(nullable: false));
            DropForeignKey("dbo.ProcessNodes", "ProcessNodeTemplateId", "dbo.ProcessNodeTemplates");
            DropForeignKey("dbo.Processes", "ProcessTemplateId", "dbo.ProcessTemplates");
            DropIndex("dbo.Criteria", new[] { "ProcessNodeTemplateId" });
            DropIndex("dbo.ProcessNodes", new[] { "ProcessNodeTemplateId" });
            DropIndex("dbo.Processes", new[] { "ProcessTemplateId" });
            AlterColumn("dbo.ProcessNodes", "ProcessNodeTemplateId", c => c.Int());
            DropColumn("dbo.ProcessTemplates", "StartingProcessNodeTemplateId");
            RenameIndex(table: "dbo.Criteria", name: "IX_CriteriaExecutionType", newName: "IX_ExecutionType");
            RenameColumn(table: "dbo.Criteria", name: "CriteriaExecutionType", newName: "ExecutionType");
        //    RenameColumn(table: "dbo.ProcessNodes", name: "ProcessNodeTemplateId", newName: "ProcessNodeTemplate_Id");
        //    RenameColumn(table: "dbo.Processes", name: "ProcessTemplateId", newName: "ProcessTemplateDO_Id");
            CreateIndex("dbo.Criteria", "ProcessNodeTemplateID");
            CreateIndex("dbo.ProcessNodes", "ProcessTemplateDO_Id");
            CreateIndex("dbo.ProcessNodes", "ProcessNodeTemplate_Id");
            CreateIndex("dbo.Processes", "ParentProcessTemplateId");
            AddForeignKey("dbo.ProcessNodes", "ProcessNodeTemplate_Id", "dbo.ProcessNodeTemplates", "Id");
            AddForeignKey("dbo.ProcessNodes", "ProcessTemplateDO_Id", "dbo.ProcessTemplates", "Id");
            AddForeignKey("dbo.Processes", "ParentProcessTemplateId", "dbo.ProcessTemplates", "Id", cascadeDelete: true);
        }
    }
}
