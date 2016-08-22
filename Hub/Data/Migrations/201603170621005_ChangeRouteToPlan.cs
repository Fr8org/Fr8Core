namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeRouteToPlan : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Routes", newName: "Plans");
            RenameTable(name: "dbo._RouteStateTemplate", newName: "_PlanStateTemplate");
            RenameTable(name: "dbo.RouteNodes", newName: "PlanNodes");
            DropForeignKey("dbo.ProcessNodes", "SubrouteId", "dbo.Subroutes");
            DropForeignKey("dbo.Criteria", "SubrouteId", "dbo.Subroutes");
            DropForeignKey("dbo.Subroutes", "Id", "dbo.RouteNodes");
            DropIndex("dbo.ProcessNodes", new[] { "SubrouteId" });
            DropIndex("dbo.Criteria", new[] { "SubrouteId" });
            DropIndex("dbo.Subroutes", new[] { "Id" });
            RenameColumn(table: "dbo.PlanNodes", name: "ParentRouteNodeId", newName: "ParentPlanNodeId");
            RenameColumn(table: "dbo.PlanNodes", name: "RootRouteNodeId", newName: "RootPlanNodeId");
            RenameColumn(table: "dbo.Containers", name: "CurrentRouteNodeId", newName: "CurrentPlanNodeId");
            RenameColumn(table: "dbo.Plans", name: "RouteState", newName: "PlanState");
            RenameIndex(table: "dbo.Containers", name: "IX_CurrentRouteNodeId", newName: "IX_CurrentPlanNodeId");
            RenameIndex(table: "dbo.PlanNodes", name: "IX_RootRouteNodeId", newName: "IX_RootPlanNodeId");
            RenameIndex(table: "dbo.PlanNodes", name: "IX_ParentRouteNodeId", newName: "IX_ParentPlanNodeId");
            RenameIndex(table: "dbo.Plans", name: "IX_RouteState", newName: "IX_PlanState");
            CreateTable(
                "dbo.SubPlans",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        StartingSubPlan = c.Boolean(nullable: false),
                        NodeTransitions = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PlanNodes", t => t.Id)
                .Index(t => t.Id);
            
            AddColumn("dbo.ProcessNodes", "SubPlanId", c => c.Guid(nullable: false));
            AddColumn("dbo.Criteria", "SubPlanId", c => c.Guid());
            CreateIndex("dbo.ProcessNodes", "SubPlanId");
            CreateIndex("dbo.Criteria", "SubPlanId");
            //AddForeignKey("dbo.ProcessNodes", "SubPlanId", "dbo.SubPlans", "Id");

            Sql("ALTER TABLE[dbo].[ProcessNodes] WITH NOCHECK ADD CONSTRAINT[FK_dbo.ProcessNodes_dbo.SubPlans_SubPlanId] FOREIGN KEY([SubPlanId]) REFERENCES[dbo].[SubPlans] ([Id])");
            AddForeignKey("dbo.Criteria", "SubPlanId", "dbo.SubPlans", "Id");
            DropColumn("dbo.ProcessNodes", "SubrouteId");
            DropColumn("dbo.Criteria", "SubrouteId");
            DropTable("dbo.Subroutes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Subroutes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        StartingSubroute = c.Boolean(nullable: false),
                        NodeTransitions = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Criteria", "SubrouteId", c => c.Guid());
            AddColumn("dbo.ProcessNodes", "SubrouteId", c => c.Guid(nullable: false));
            DropForeignKey("dbo.SubPlans", "Id", "dbo.PlanNodes");
            DropForeignKey("dbo.Criteria", "SubPlanId", "dbo.SubPlans");
            DropForeignKey("dbo.ProcessNodes", "SubPlanId", "dbo.SubPlans");
            DropIndex("dbo.SubPlans", new[] { "Id" });
            DropIndex("dbo.Criteria", new[] { "SubPlanId" });
            DropIndex("dbo.ProcessNodes", new[] { "SubPlanId" });
            DropColumn("dbo.Criteria", "SubPlanId");
            DropColumn("dbo.ProcessNodes", "SubPlanId");
            DropTable("dbo.SubPlans");
            RenameIndex(table: "dbo.Plans", name: "IX_PlanState", newName: "IX_RouteState");
            RenameIndex(table: "dbo.PlanNodes", name: "IX_ParentPlanNodeId", newName: "IX_ParentRouteNodeId");
            RenameIndex(table: "dbo.PlanNodes", name: "IX_RootPlanNodeId", newName: "IX_RootRouteNodeId");
            RenameIndex(table: "dbo.Containers", name: "IX_CurrentPlanNodeId", newName: "IX_CurrentRouteNodeId");
            RenameColumn(table: "dbo.Plans", name: "PlanState", newName: "RouteState");
            RenameColumn(table: "dbo.Containers", name: "CurrentPlanNodeId", newName: "CurrentRouteNodeId");
            RenameColumn(table: "dbo.PlanNodes", name: "RootPlanNodeId", newName: "RootRouteNodeId");
            RenameColumn(table: "dbo.PlanNodes", name: "ParentPlanNodeId", newName: "ParentRouteNodeId");
            CreateIndex("dbo.Subroutes", "Id");
            CreateIndex("dbo.Criteria", "SubrouteId");
            CreateIndex("dbo.ProcessNodes", "SubrouteId");
            AddForeignKey("dbo.Subroutes", "Id", "dbo.RouteNodes", "Id");
            AddForeignKey("dbo.Criteria", "SubrouteId", "dbo.Subroutes", "Id");
            AddForeignKey("dbo.ProcessNodes", "SubrouteId", "dbo.Subroutes", "Id");
            RenameTable(name: "dbo.PlanNodes", newName: "RouteNodes");
            RenameTable(name: "dbo._PlanStateTemplate", newName: "_RouteStateTemplate");
            RenameTable(name: "dbo.Plans", newName: "Routes");
        }
    }
}
