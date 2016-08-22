namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameContainerFields : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Containers", name: "ContainerState", newName: "State");
            RenameColumn(table: "dbo.Containers", name: "CurrentPlanNodeId", newName: "CurrentActivityId");
            RenameColumn(table: "dbo.Containers", name: "NextRouteNodeId", newName: "NextActivityId");
            RenameIndex(table: "dbo.Containers", name: "IX_ContainerState", newName: "IX_State");
            RenameIndex(table: "dbo.Containers", name: "IX_CurrentPlanNodeId", newName: "IX_CurrentActivityId");
            RenameIndex(table: "dbo.Containers", name: "IX_NextRouteNodeId", newName: "IX_NextActivityId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Containers", name: "IX_NextActivityId", newName: "IX_NextRouteNodeId");
            RenameIndex(table: "dbo.Containers", name: "IX_CurrentActivityId", newName: "IX_CurrentPlanNodeId");
            RenameIndex(table: "dbo.Containers", name: "IX_State", newName: "IX_ContainerState");
            RenameColumn(table: "dbo.Containers", name: "NextActivityId", newName: "NextRouteNodeId");
            RenameColumn(table: "dbo.Containers", name: "CurrentActivityId", newName: "CurrentPlanNodeId");
            RenameColumn(table: "dbo.Containers", name: "State", newName: "ContainerState");
        }
    }
}
