namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProcessNodesRemoved : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ProcessNodes", "ParentContainerId", "dbo.Containers");
            DropForeignKey("dbo.ProcessNodes", "ProcessNodeState", "dbo._ProcessNodeStatusTemplate");
            DropForeignKey("dbo.ProcessNodes", "SubPlanId", "dbo.SubPlans");
            DropIndex("dbo.ProcessNodes", new[] { "ParentContainerId" });
            DropIndex("dbo.ProcessNodes", new[] { "ProcessNodeState" });
            DropIndex("dbo.ProcessNodes", new[] { "SubPlanId" });
            DropTable("dbo.ProcessNodes");
            DropTable("dbo._ProcessNodeStatusTemplate");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo._ProcessNodeStatusTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProcessNodes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ParentContainerId = c.Guid(nullable: false),
                        ProcessNodeState = c.Int(),
                        SubPlanId = c.Guid(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.ProcessNodes", "SubPlanId");
            CreateIndex("dbo.ProcessNodes", "ProcessNodeState");
            CreateIndex("dbo.ProcessNodes", "ParentContainerId");
            AddForeignKey("dbo.ProcessNodes", "SubPlanId", "dbo.SubPlans", "Id");
            AddForeignKey("dbo.ProcessNodes", "ProcessNodeState", "dbo._ProcessNodeStatusTemplate", "Id");
            AddForeignKey("dbo.ProcessNodes", "ParentContainerId", "dbo.Containers", "Id");
        }
    }
}
