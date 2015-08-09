namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCascadeForAttendeeOnNegotiations : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Attendees", "NegotiationID", "dbo.Negotiations");
            AddForeignKey("dbo.Attendees", "NegotiationID", "dbo.Negotiations", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Attendees", "NegotiationID", "dbo.Negotiations");
            AddForeignKey("dbo.Attendees", "NegotiationID", "dbo.Negotiations", "Id");
        }
    }
}
