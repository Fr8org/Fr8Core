namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class LinkCalendarToNegotiation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Calendars", "ClarificationRequestID", c => c.Int());
            CreateIndex("dbo.Calendars", "ClarificationRequestID");
            AddForeignKey("dbo.Calendars", "ClarificationRequestID", "dbo.ClarificationRequests", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Calendars", "ClarificationRequestID", "dbo.ClarificationRequests");
            DropIndex("dbo.Calendars", new[] { "ClarificationRequestID" });
            DropColumn("dbo.Calendars", "ClarificationRequestID");
        }
    }
}
