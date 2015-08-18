namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update_Process_ProcessNode_Relation : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Processes", new[] { "ProcessNode_Id" });
            DropIndex("dbo.ProcessNodes", new[] { "ParentProcessId" });
            DropColumn("dbo.Processes", "CurrentProcessNodeId");
            RenameColumn(table: "dbo.Processes", name: "ProcessNode_Id", newName: "CurrentProcessNodeId");
            AddColumn("dbo.ProcessNodeTemplates", "NodeTransitions", c => c.String());
            AddColumn("dbo.ProcessTemplates", "DockyardAccount_Id", c => c.String(maxLength: 128));
            AlterColumn("dbo.Processes", "CurrentProcessNodeId", c => c.Int());
            AlterColumn("dbo.ProcessNodes", "ParentProcessId", c => c.Int());
            CreateIndex("dbo.Processes", "CurrentProcessNodeId");
            CreateIndex("dbo.ProcessNodes", "ParentProcessId");
            CreateIndex("dbo.ProcessTemplates", "DockyardAccount_Id");
            AddForeignKey("dbo.ProcessTemplates", "DockyardAccount_Id", "dbo.Users", "Id");
            DropColumn("dbo.ProcessNodeTemplates", "TransitionKey");
            DropColumn("dbo.ProcessTemplates", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProcessTemplates", "UserId", c => c.String(nullable: false));
            AddColumn("dbo.ProcessNodeTemplates", "TransitionKey", c => c.String());
            DropForeignKey("dbo.ProcessTemplates", "DockyardAccount_Id", "dbo.Users");
            DropIndex("dbo.ProcessTemplates", new[] { "DockyardAccount_Id" });
            DropIndex("dbo.ProcessNodes", new[] { "ParentProcessId" });
            DropIndex("dbo.Processes", new[] { "CurrentProcessNodeId" });
            AlterColumn("dbo.ProcessNodes", "ParentProcessId", c => c.Int(nullable: false));
            AlterColumn("dbo.Processes", "CurrentProcessNodeId", c => c.Int(nullable: false));
            DropColumn("dbo.ProcessTemplates", "DockyardAccount_Id");
            DropColumn("dbo.ProcessNodeTemplates", "NodeTransitions");
            RenameColumn(table: "dbo.Processes", name: "CurrentProcessNodeId", newName: "ProcessNode_Id");
            AddColumn("dbo.Processes", "CurrentProcessNodeId", c => c.Int(nullable: false));
            CreateIndex("dbo.ProcessNodes", "ParentProcessId");
            CreateIndex("dbo.Processes", "ProcessNode_Id");
        }
    }
}
