namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Rename_Activity_To_RouteNode_Migration : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ActivityDOes", "ParentActivityId", "dbo.ActivityDOes");
            DropForeignKey("dbo.Containers", "CurrentActivityId", "dbo.ActivityDOes");
            DropForeignKey("dbo.Containers", "NextActivityId", "dbo.ActivityDOes");
            DropForeignKey("dbo.Routes", "Id", "dbo.ActivityDOes");
            DropForeignKey("dbo.Actions", "Id", "dbo.ActivityDOes");
            DropForeignKey("dbo.Subroutes", "Id", "dbo.ActivityDOes");
            DropIndex("dbo.Containers", new[] { "CurrentActivityId" });
            DropIndex("dbo.Containers", new[] { "NextActivityId" });
            DropIndex("dbo.ActivityDOes", new[] { "ParentActivityId" });
            CreateTable(
                "dbo.RouteNodes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParentRouteNodeId = c.Int(),
                        Ordering = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RouteNodes", t => t.ParentRouteNodeId)
                .Index(t => t.ParentRouteNodeId);
            
            AddColumn("dbo.Containers", "Fr8AccountId", c => c.String());
            AddColumn("dbo.Containers", "CurrentRouteNodeId", c => c.Int());
            AddColumn("dbo.Containers", "NextRouteNodeId", c => c.Int());
            CreateIndex("dbo.Containers", "CurrentRouteNodeId");
            CreateIndex("dbo.Containers", "NextRouteNodeId");
            AddForeignKey("dbo.Containers", "CurrentRouteNodeId", "dbo.RouteNodes", "Id");
            AddForeignKey("dbo.Containers", "NextRouteNodeId", "dbo.RouteNodes", "Id");
            AddForeignKey("dbo.Routes", "Id", "dbo.RouteNodes", "Id");
            AddForeignKey("dbo.Actions", "Id", "dbo.RouteNodes", "Id");
            AddForeignKey("dbo.Subroutes", "Id", "dbo.RouteNodes", "Id");
            DropColumn("dbo.Containers", "DockyardAccountId");
            DropColumn("dbo.Containers", "CurrentActivityId");
            DropColumn("dbo.Containers", "NextActivityId");
            DropTable("dbo.ActivityDOes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ActivityDOes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParentActivityId = c.Int(),
                        Ordering = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Containers", "NextActivityId", c => c.Int());
            AddColumn("dbo.Containers", "CurrentActivityId", c => c.Int());
            AddColumn("dbo.Containers", "DockyardAccountId", c => c.String());
            DropForeignKey("dbo.Subroutes", "Id", "dbo.RouteNodes");
            DropForeignKey("dbo.Actions", "Id", "dbo.RouteNodes");
            DropForeignKey("dbo.Routes", "Id", "dbo.RouteNodes");
            DropForeignKey("dbo.Containers", "NextRouteNodeId", "dbo.RouteNodes");
            DropForeignKey("dbo.Containers", "CurrentRouteNodeId", "dbo.RouteNodes");
            DropForeignKey("dbo.RouteNodes", "ParentRouteNodeId", "dbo.RouteNodes");
            DropIndex("dbo.RouteNodes", new[] { "ParentRouteNodeId" });
            DropIndex("dbo.Containers", new[] { "NextRouteNodeId" });
            DropIndex("dbo.Containers", new[] { "CurrentRouteNodeId" });
            DropColumn("dbo.Containers", "NextRouteNodeId");
            DropColumn("dbo.Containers", "CurrentRouteNodeId");
            DropColumn("dbo.Containers", "Fr8AccountId");
            DropTable("dbo.RouteNodes");
            CreateIndex("dbo.ActivityDOes", "ParentActivityId");
            CreateIndex("dbo.Containers", "NextActivityId");
            CreateIndex("dbo.Containers", "CurrentActivityId");
            AddForeignKey("dbo.Subroutes", "Id", "dbo.ActivityDOes", "Id");
            AddForeignKey("dbo.Actions", "Id", "dbo.ActivityDOes", "Id");
            AddForeignKey("dbo.Routes", "Id", "dbo.ActivityDOes", "Id");
            AddForeignKey("dbo.Containers", "NextActivityId", "dbo.ActivityDOes", "Id");
            AddForeignKey("dbo.Containers", "CurrentActivityId", "dbo.ActivityDOes", "Id");
            AddForeignKey("dbo.ActivityDOes", "ParentActivityId", "dbo.ActivityDOes", "Id");
        }
    }
}
