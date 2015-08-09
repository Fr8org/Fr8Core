namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class UseEmailAddressDOOnAttendees : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Attendees", "EmailAddressID", c => c.Int(nullable: false));
            Sql(@"-- Remove invalid data
delete from attendees where EmailAddress is null

--Create missing email address rows
insert into EmailAddresses
select EmailAddress,EmailAddress from attendees
LEFT JOIN EmailAddresses on EmailAddress = Address
WHERE EmailAddresses.ID is null

--Setup the foreign key
update attendees set EmailAddressID = EmailAddresses.ID
from attendees
inner join EmailAddresses on EmailAddress = Address");

            CreateIndex("dbo.Attendees", "EmailAddressID");
            AddForeignKey("dbo.Attendees", "EmailAddressID", "dbo.EmailAddresses", "Id", cascadeDelete: true);
            DropColumn("dbo.Attendees", "EmailAddress");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Attendees", "EmailAddress", c => c.String());
            DropForeignKey("dbo.Attendees", "EmailAddressID", "dbo.EmailAddresses");
            DropIndex("dbo.Attendees", new[] { "EmailAddressID" });
            DropColumn("dbo.Attendees", "EmailAddressID");
        }
    }
}
