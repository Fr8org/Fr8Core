namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class RenameStatusAgain : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.BookingRequests", name: "BRStateID", newName: "BRState");
            RenameIndex(table: "dbo.BookingRequests", name: "IX_BRStateID", newName: "IX_BRState");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.BookingRequests", name: "IX_BRState", newName: "IX_BRStateID");
            RenameColumn(table: "dbo.BookingRequests", name: "BRState", newName: "BRStateID");
        }
    }
}
