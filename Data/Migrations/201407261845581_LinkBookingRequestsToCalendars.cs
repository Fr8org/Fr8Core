namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class LinkBookingRequestsToCalendars : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BookingRequestCalendar",
                c => new
                    {
                        BookingRequestID = c.Int(nullable: false),
                        CalendarID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.BookingRequestID, t.CalendarID })
                .ForeignKey("dbo.BookingRequests", t => t.BookingRequestID, cascadeDelete: true)
                .ForeignKey("dbo.Calendars", t => t.CalendarID, cascadeDelete: true)
                .Index(t => t.BookingRequestID)
                .Index(t => t.CalendarID);

            //Link all calendars of a user to their booking requests
            Sql(@"INSERT INTO BookingRequestCalendar
select BookingRequests.ID, Calendars.ID from BookingRequests
INNER JOIN Users on User_ID = Users.Id
INNER JOIN Calendars on OwnerID = Users.ID
");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BookingRequestCalendar", "CalendarID", "dbo.Calendars");
            DropForeignKey("dbo.BookingRequestCalendar", "BookingRequestID", "dbo.BookingRequests");
            DropIndex("dbo.BookingRequestCalendar", new[] { "CalendarID" });
            DropIndex("dbo.BookingRequestCalendar", new[] { "BookingRequestID" });
            DropTable("dbo.BookingRequestCalendar");
        }
    }
}
