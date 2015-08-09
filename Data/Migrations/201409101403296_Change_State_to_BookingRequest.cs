namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Change_State_to_BookingRequest : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.BookingRequests", name: "BookingRequestState", newName: "State");
            RenameIndex(table: "dbo.BookingRequests", name: "IX_BookingRequestState", newName: "IX_State");
            AlterColumn("dbo.Facts", "BookerId", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Facts", "BookerId", c => c.Int(nullable: false));
            RenameIndex(table: "dbo.BookingRequests", name: "IX_State", newName: "IX_BookingRequestState");
            RenameColumn(table: "dbo.BookingRequests", name: "State", newName: "BookingRequestState");
        }
    }
}
