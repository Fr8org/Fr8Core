namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class FixingEventModel : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Events", new[] { "BookingRequest_Id" });
            RenameColumn(table: "dbo.Events", name: "BookingRequest_Id", newName: "BookingRequestID");
            AlterColumn("dbo.Events", "BookingRequestID", c => c.Int(nullable: false));
            CreateIndex("dbo.Events", "BookingRequestID");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Events", new[] { "BookingRequestID" });
            AlterColumn("dbo.Events", "BookingRequestID", c => c.Int());
            RenameColumn(table: "dbo.Events", name: "BookingRequestID", newName: "BookingRequest_Id");
            CreateIndex("dbo.Events", "BookingRequest_Id");
        }
    }
}
