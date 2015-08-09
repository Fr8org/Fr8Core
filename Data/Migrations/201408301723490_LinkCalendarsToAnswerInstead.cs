namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LinkCalendarsToAnswerInstead : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Questions", "CalendarID", "dbo.Calendars");
            DropIndex("dbo.Questions", new[] { "CalendarID" });
            AddColumn("dbo.Answers", "CalendarID", c => c.Int());
            CreateIndex("dbo.Answers", "CalendarID");
            AddForeignKey("dbo.Answers", "CalendarID", "dbo.Calendars", "Id");
            DropColumn("dbo.Questions", "CalendarID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Questions", "CalendarID", c => c.Int());
            DropForeignKey("dbo.Answers", "CalendarID", "dbo.Calendars");
            DropIndex("dbo.Answers", new[] { "CalendarID" });
            DropColumn("dbo.Answers", "CalendarID");
            CreateIndex("dbo.Questions", "CalendarID");
            AddForeignKey("dbo.Questions", "CalendarID", "dbo.Calendars", "Id");
        }
    }
}
