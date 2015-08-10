namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewNegotiationStructure : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Answers", "CalendarID", "dbo.Calendars");
            DropIndex("dbo.Answers", new[] { "CalendarID" });
            AddColumn("dbo.Questions", "CalendarID", c => c.Int());
            AddColumn("dbo.Answers", "EventID", c => c.Int());
            CreateIndex("dbo.Questions", "CalendarID");
            CreateIndex("dbo.Answers", "EventID");
            AddForeignKey("dbo.Answers", "EventID", "dbo.Events", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Questions", "CalendarID", "dbo.Calendars", "Id");
            DropColumn("dbo.Answers", "CalendarID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Answers", "CalendarID", c => c.Int());
            DropForeignKey("dbo.Questions", "CalendarID", "dbo.Calendars");
            DropForeignKey("dbo.Answers", "EventID", "dbo.Events");
            DropIndex("dbo.Answers", new[] { "EventID" });
            DropIndex("dbo.Questions", new[] { "CalendarID" });
            DropColumn("dbo.Answers", "EventID");
            DropColumn("dbo.Questions", "CalendarID");
            CreateIndex("dbo.Answers", "CalendarID");
            AddForeignKey("dbo.Answers", "CalendarID", "dbo.Calendars", "Id");
        }
    }
}
