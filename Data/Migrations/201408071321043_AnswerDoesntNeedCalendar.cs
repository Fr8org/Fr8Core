namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AnswerDoesntNeedCalendar : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Answers", "CalendarID", "dbo.Calendars");
            DropIndex("dbo.Answers", new[] { "CalendarID" });
            AlterColumn("dbo.Answers", "CalendarID", c => c.Int());
            CreateIndex("dbo.Answers", "CalendarID");
            AddForeignKey("dbo.Answers", "CalendarID", "dbo.Calendars", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Answers", "CalendarID", "dbo.Calendars");
            DropIndex("dbo.Answers", new[] { "CalendarID" });
            AlterColumn("dbo.Answers", "CalendarID", c => c.Int(nullable: false));
            CreateIndex("dbo.Answers", "CalendarID");
            AddForeignKey("dbo.Answers", "CalendarID", "dbo.Calendars", "Id", cascadeDelete: false);
        }
    }
}
