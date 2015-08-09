namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class LinkAnswerToCalendar : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Answers", "CalendarID", c => c.Int(nullable: false));
            CreateIndex("dbo.Answers", "CalendarID");
            AddForeignKey("dbo.Answers", "CalendarID", "dbo.Calendars", "Id", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Answers", "CalendarID", "dbo.Calendars");
            DropIndex("dbo.Answers", new[] { "CalendarID" });
            DropColumn("dbo.Answers", "CalendarID");
        }
    }
}
