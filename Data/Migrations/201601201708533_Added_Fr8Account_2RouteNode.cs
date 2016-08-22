namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_Fr8Account_2RouteNode : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
    
            AddColumn("dbo.RouteNodes", "Fr8Account_TEMP_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.RouteNodes", "Fr8Account_TEMP_Id");
            AddForeignKey("dbo.RouteNodes", "Fr8Account_TEMP_Id", "dbo.Users", "Id");

            Sql(@"update dbo.RouteNodes
                  set dbo.RouteNodes.Fr8Account_TEMP_Id = r.Fr8Account_Id
                  from RouteNodes rn
                  inner join Routes r
                  on rn.RootRouteNodeId = r.Id ");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RouteNodes", "Fr8Account_TEMP_Id", "dbo.Users");
            DropIndex("dbo.RouteNodes", new[] { "Fr8Account_TEMP_Id" });
            DropColumn("dbo.RouteNodes", "Fr8Account_TEMP_Id");
        }
    }
}
