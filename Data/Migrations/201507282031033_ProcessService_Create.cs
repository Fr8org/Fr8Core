namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProcessService_Create : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ProcessNodes", new[] { "ParentProcess_Id" });
            RenameColumn(table: "dbo.ProcessNodes", name: "ParentProcess_Id", newName: "ProcessID");
            AddColumn("dbo.Processes", "EnvelopeId", c => c.String());
            AddColumn("dbo.Processes", "ProcessNodeID", c => c.Int(nullable: false));
            AlterColumn("dbo.ProcessNodes", "ProcessID", c => c.Int(nullable: false));
            CreateIndex("dbo.Processes", "ProcessNodeID");
            CreateIndex("dbo.ProcessNodes", "ProcessID");
            AddForeignKey("dbo.Processes", "ProcessNodeID", "dbo.ProcessNodes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Processes", "ProcessNodeID", "dbo.ProcessNodes");
            DropIndex("dbo.ProcessNodes", new[] { "ProcessID" });
            DropIndex("dbo.Processes", new[] { "ProcessNodeID" });
            AlterColumn("dbo.ProcessNodes", "ProcessID", c => c.Int());
            DropColumn("dbo.Processes", "ProcessNodeID");
            DropColumn("dbo.Processes", "EnvelopeId");
            RenameColumn(table: "dbo.ProcessNodes", name: "ProcessID", newName: "ParentProcess_Id");
            CreateIndex("dbo.ProcessNodes", "ParentProcess_Id");
        }
    }
}
