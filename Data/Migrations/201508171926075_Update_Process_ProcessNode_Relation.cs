namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update_Process_ProcessNode_Relation : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Processes", new[] { "ProcessNode_Id" });
            DropColumn("dbo.Processes", "CurrentProcessNodeId");
            RenameColumn(table: "dbo.Processes", name: "ProcessNode_Id", newName: "CurrentProcessNodeId");
            AddColumn("dbo.ProcessNodeTemplates", "NodeTransitions", c => c.String());
            AlterColumn("dbo.Processes", "CurrentProcessNodeId", c => c.Int());
            CreateIndex("dbo.Processes", "CurrentProcessNodeId");
            DropColumn("dbo.ProcessNodeTemplates", "TransitionKey");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProcessNodeTemplates", "TransitionKey", c => c.String());
            DropIndex("dbo.Processes", new[] { "CurrentProcessNodeId" });
            AlterColumn("dbo.Processes", "CurrentProcessNodeId", c => c.Int(nullable: false));
            DropColumn("dbo.ProcessNodeTemplates", "NodeTransitions");
            RenameColumn(table: "dbo.Processes", name: "CurrentProcessNodeId", newName: "ProcessNode_Id");
            AddColumn("dbo.Processes", "CurrentProcessNodeId", c => c.Int(nullable: false));
            CreateIndex("dbo.Processes", "ProcessNode_Id");
        }
    }
}
