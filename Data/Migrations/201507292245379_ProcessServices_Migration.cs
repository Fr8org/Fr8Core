namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProcessServices_Migration : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Processes", "StartingProcessNodeTemplate_Id", "dbo.ProcessNodeTemplates");
            DropIndex("dbo.Processes", new[] { "StartingProcessNodeTemplate_Id" });
            DropIndex("dbo.ProcessNodes", new[] { "ParentProcess_Id" });
            RenameColumn(table: "dbo.ProcessNodeTemplates", name: "ProcessTemplateID", newName: "ParentTemplateId");
            RenameColumn(table: "dbo.ProcessTemplates", name: "ProcessState", newName: "ProcessTemplateState");
            RenameColumn(table: "dbo.ProcessNodes", name: "ParentProcess_Id", newName: "ParentProcessId");
            RenameIndex(table: "dbo.ProcessNodeTemplates", name: "IX_ProcessTemplateID", newName: "IX_ParentTemplateId");
            RenameIndex(table: "dbo.ProcessTemplates", name: "IX_ProcessState", newName: "IX_ProcessTemplateState");
            CreateTable(
                "dbo._ProcessNodeStatusTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Processes", "DockyardAccountId", c => c.String());
            AddColumn("dbo.Processes", "EnvelopeId", c => c.String());
            AddColumn("dbo.Processes", "CurrentProcessNodeId", c => c.Int(nullable: false));
            AddColumn("dbo.Processes", "ProcessNode_Id", c => c.Int());
            AddColumn("dbo.ProcessNodes", "ProcessNodeState", c => c.Int());
            AlterColumn("dbo.ProcessNodes", "ParentProcessId", c => c.Int(nullable: false));
            CreateIndex("dbo.Processes", "ProcessNode_Id");
            CreateIndex("dbo.ProcessNodes", "ParentProcessId");
            CreateIndex("dbo.ProcessNodes", "ProcessNodeState");
            AddForeignKey("dbo.ProcessNodes", "ProcessNodeState", "dbo._ProcessNodeStatusTemplate", "Id");
            AddForeignKey("dbo.Processes", "ProcessNode_Id", "dbo.ProcessNodes", "Id");
            DropColumn("dbo.Processes", "Description");
            DropColumn("dbo.Processes", "UserId");
            DropColumn("dbo.Processes", "StartingProcessNodeTemplate_Id");
            DropColumn("dbo.ProcessTemplates", "UserId");
            DropColumn("dbo.ProcessNodes", "State");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProcessNodes", "State", c => c.Int(nullable: false));
            AddColumn("dbo.ProcessTemplates", "UserId", c => c.String());
            AddColumn("dbo.Processes", "StartingProcessNodeTemplate_Id", c => c.Int());
            AddColumn("dbo.Processes", "UserId", c => c.String());
            AddColumn("dbo.Processes", "Description", c => c.String());
            DropForeignKey("dbo.Processes", "ProcessNode_Id", "dbo.ProcessNodes");
            DropForeignKey("dbo.ProcessNodes", "ProcessNodeState", "dbo._ProcessNodeStatusTemplate");
            DropIndex("dbo.ProcessNodes", new[] { "ProcessNodeState" });
            DropIndex("dbo.ProcessNodes", new[] { "ParentProcessId" });
            DropIndex("dbo.Processes", new[] { "ProcessNode_Id" });
            AlterColumn("dbo.ProcessNodes", "ParentProcessId", c => c.Int());
            DropColumn("dbo.ProcessNodes", "ProcessNodeState");
            DropColumn("dbo.Processes", "ProcessNode_Id");
            DropColumn("dbo.Processes", "CurrentProcessNodeId");
            DropColumn("dbo.Processes", "EnvelopeId");
            DropColumn("dbo.Processes", "DockyardAccountId");
            DropTable("dbo._ProcessNodeStatusTemplate");
            RenameIndex(table: "dbo.ProcessTemplates", name: "IX_ProcessTemplateState", newName: "IX_ProcessState");
            RenameIndex(table: "dbo.ProcessNodeTemplates", name: "IX_ParentTemplateId", newName: "IX_ProcessTemplateID");
            RenameColumn(table: "dbo.ProcessNodes", name: "ParentProcessId", newName: "ParentProcess_Id");
            RenameColumn(table: "dbo.ProcessTemplates", name: "ProcessTemplateState", newName: "ProcessState");
            RenameColumn(table: "dbo.ProcessNodeTemplates", name: "ParentTemplateId", newName: "ProcessTemplateID");
            CreateIndex("dbo.ProcessNodes", "ParentProcess_Id");
            CreateIndex("dbo.Processes", "StartingProcessNodeTemplate_Id");
            AddForeignKey("dbo.Processes", "StartingProcessNodeTemplate_Id", "dbo.ProcessNodeTemplates", "Id");
        }
    }
}
