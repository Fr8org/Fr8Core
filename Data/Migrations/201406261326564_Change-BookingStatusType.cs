namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class ChangeBookingStatusType : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BookingRequestStatuses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.BookingRequests", "BookingRequestStatusID", c => c.Int(nullable: false));

            Sql(@"SET IDENTITY_INSERT dbo.BookingRequestStatuses ON;");

            Sql(@"Insert into BookingRequestStatuses (ID,NAME) VALUES (1,'Unprocessed')");
            Sql(@"Insert into BookingRequestStatuses (ID,NAME) VALUES (2,'Processed')");
            Sql(@"Insert into BookingRequestStatuses (ID,NAME) VALUES (3,'CheckedOut')");
            Sql(@"Insert into BookingRequestStatuses (ID,NAME) VALUES (4,'Invalid')");

            Sql(@"SET IDENTITY_INSERT dbo.BookingRequestStatuses OFF;");

            //Set all 'Unknowns' to 'Processed' by default
            Sql(@"Update BookingRequests set BookingRequestStatusID = 2");

            Sql(@"Update BookingRequests set BookingRequestStatusID = 1 WHERE BookingStatus = 'Unprocessed'");
            Sql(@"Update BookingRequests set BookingRequestStatusID = 2 WHERE BookingStatus = 'Processed'");
            Sql(@"Update BookingRequests set BookingRequestStatusID = 3 WHERE BookingStatus = 'CheckedOut'");
            Sql(@"Update BookingRequests set BookingRequestStatusID = 4 WHERE BookingStatus = 'Invalid'");
            
            CreateIndex("dbo.BookingRequests", "BookingRequestStatusID");
            AddForeignKey("dbo.BookingRequests", "BookingRequestStatusID", "dbo.BookingRequestStatuses", "Id", cascadeDelete: true);
            DropColumn("dbo.BookingRequests", "BookingStatus");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BookingRequests", "BookingStatus", c => c.String(nullable: false));
            DropForeignKey("dbo.BookingRequests", "BookingRequestStatusID", "dbo.BookingRequestStatuses");
            DropIndex("dbo.BookingRequests", new[] { "BookingRequestStatusID" });
            DropColumn("dbo.BookingRequests", "BookingRequestStatusID");
            DropTable("dbo.BookingRequestStatuses");
        }
    }
}
