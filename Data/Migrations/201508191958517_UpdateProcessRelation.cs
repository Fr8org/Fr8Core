namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateProcessRelation : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Processes", "ProcessNode_Id", "dbo.ProcessNodes");
            DropIndex("dbo.Processes", new[] { "ProcessNode_Id" });
            AddColumn("dbo.ProcessNodeTemplates", "NodeTransitions", c => c.String());
            AddColumn("dbo.ProcessTemplates", "DockyardAccount_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.ProcessTemplates", "DockyardAccount_Id");
            AddForeignKey("dbo.ProcessTemplates", "DockyardAccount_Id", "dbo.Users", "Id");
            DropColumn("dbo.Processes", "CurrentProcessNodeId");
            DropColumn("dbo.Processes", "ProcessNode_Id");
            DropColumn("dbo.ProcessNodeTemplates", "TransitionKey");
            DropColumn("dbo.ProcessTemplates", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ProcessTemplates", "UserId", c => c.String(nullable: false));
            AddColumn("dbo.ProcessNodeTemplates", "TransitionKey", c => c.String());
            AddColumn("dbo.Processes", "ProcessNode_Id", c => c.Int());
            AddColumn("dbo.Processes", "CurrentProcessNodeId", c => c.Int(nullable: false));
            DropForeignKey("dbo.ProcessTemplates", "DockyardAccount_Id", "dbo.Users");
            DropIndex("dbo.ProcessTemplates", new[] { "DockyardAccount_Id" });
            DropColumn("dbo.ProcessTemplates", "DockyardAccount_Id");
            DropColumn("dbo.ProcessNodeTemplates", "NodeTransitions");
            CreateIndex("dbo.Processes", "ProcessNode_Id");
            AddForeignKey("dbo.Processes", "ProcessNode_Id", "dbo.ProcessNodes", "Id");
        }
    }
}
