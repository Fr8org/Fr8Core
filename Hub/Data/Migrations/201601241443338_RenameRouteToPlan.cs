namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameRouteToPlan : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Containers", name: "RouteId", newName: "PlanId");
            RenameIndex(table: "dbo.Containers", name: "IX_RouteId", newName: "IX_PlanId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Containers", name: "IX_PlanId", newName: "IX_RouteId");
            RenameColumn(table: "dbo.Containers", name: "PlanId", newName: "RouteId");
        }
    }
}
