namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixBookingRequestState : DbMigration
    {
        public override void Up()
        {
            //'Unstarted' state was removed. Update any booking request with that state to be 'NeedsBooking'
            Sql("UPDATE dbo.BookingRequests SET State = 6 WHERE State = 1");
        }
        
        public override void Down()
        {
        }
    }
}
