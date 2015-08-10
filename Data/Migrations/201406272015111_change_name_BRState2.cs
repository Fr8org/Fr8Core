namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class change_name_BRState2 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.BookingRequests", name: "BookingRequestStatusID", newName: "BRState");
            RenameIndex(table: "dbo.BookingRequests", name: "IX_BookingRequestStatusID", newName: "IX_BRState");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.BookingRequests", name: "IX_BRState", newName: "IX_BookingRequestStatusID");
            RenameColumn(table: "dbo.BookingRequests", name: "BRState", newName: "BookingRequestStatusID");
        }
    }
}
