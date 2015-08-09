using Data.States;

namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBRAvailabilityAndPB : KwasantDbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo._BookingRequestAvailabilityTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);

            SeedConstants<BookingRequestAvailability>("dbo._BookingRequestAvailabilityTemplate");
            
            AddColumn("dbo.BookingRequests", "PreferredBookerID", c => c.String(maxLength: 128));
            AddColumn("dbo.BookingRequests", "Availability", c => c.Int());
            CreateIndex("dbo.BookingRequests", "PreferredBookerID");
            CreateIndex("dbo.BookingRequests", "Availability");
            AddForeignKey("dbo.BookingRequests", "PreferredBookerID", "dbo.Users", "Id");
            AddForeignKey("dbo.BookingRequests", "Availability", "dbo._BookingRequestAvailabilityTemplate", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BookingRequests", "Availability", "dbo._BookingRequestAvailabilityTemplate");
            DropForeignKey("dbo.BookingRequests", "PreferredBookerID", "dbo.Users");
            DropIndex("dbo.BookingRequests", new[] { "Availability" });
            DropIndex("dbo.BookingRequests", new[] { "PreferredBookerID" });
            DropColumn("dbo.BookingRequests", "Availability");
            DropColumn("dbo.BookingRequests", "PreferredBookerID");
            DropTable("dbo._BookingRequestAvailabilityTemplate");
        }
    }
}
