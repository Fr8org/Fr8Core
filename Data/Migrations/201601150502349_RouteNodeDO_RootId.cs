namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RouteNodeDO_RootId : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RouteNodes", "RootRouteNodeId", c => c.Guid());
            CreateIndex("dbo.RouteNodes", "RootRouteNodeId");
            AddForeignKey("dbo.RouteNodes", "RootRouteNodeId", "dbo.RouteNodes", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RouteNodes", "RootRouteNodeId", "dbo.RouteNodes");
            DropIndex("dbo.RouteNodes", new[] { "RootRouteNodeId" });
            DropColumn("dbo.RouteNodes", "RootRouteNodeId");
        }
    }
}
