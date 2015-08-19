namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Update_Process_ProcessNode_Relation : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Processes", "ProcessNode_Id", "dbo.ProcessNodes");
            DropIndex("dbo.Processes", new[] { "ProcessNode_Id" });
            AddColumn("dbo.ProcessNodeTemplates", "NodeTransitions", c => c.String());
            DropColumn("dbo.Processes", "CurrentProcessNodeId");
            DropColumn("dbo.Processes", "ProcessNode_Id");
            DropColumn("dbo.ProcessNodeTemplates", "TransitionKey");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProcessNodeTemplates", "TransitionKey", c => c.String());
            AddColumn("dbo.Processes", "ProcessNode_Id", c => c.Int());
            AddColumn("dbo.Processes", "CurrentProcessNodeId", c => c.Int(nullable: false));
            DropColumn("dbo.ProcessNodeTemplates", "NodeTransitions");
            CreateIndex("dbo.Processes", "ProcessNode_Id");
            AddForeignKey("dbo.Processes", "ProcessNode_Id", "dbo.ProcessNodes", "Id");
        }
    }
}
