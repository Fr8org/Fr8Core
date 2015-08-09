namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameBRUserToCustomer : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.BookingRequests", name: "UserID", newName: "CustomerID");
            RenameIndex(table: "dbo.BookingRequests", name: "IX_UserID", newName: "IX_CustomerID");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.BookingRequests", name: "IX_CustomerID", newName: "IX_UserID");
            RenameColumn(table: "dbo.BookingRequests", name: "CustomerID", newName: "UserID");
        }
    }
}
