namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class MoveCalendarIDToQuestionDO : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Answers", "CalendarID", "dbo.Calendars");
            DropForeignKey("dbo.Calendars", "QuestionId", "dbo.Questions");
            DropIndex("dbo.Calendars", new[] { "QuestionId" });
            DropIndex("dbo.Answers", new[] { "CalendarID" });
            AddColumn("dbo.Questions", "CalendarID", c => c.Int(nullable: true));
            CreateIndex("dbo.Questions", "CalendarID");
            AddForeignKey("dbo.Questions", "CalendarID", "dbo.Calendars", "Id");

            Sql(@"
-- MAX isn't important. Our current state of the DB should mean that each question can only have 1 'EventWindow' answer. 
-- We use MAX to ensure we return only 1 CalendarID, since it's a 1..* relationship

UPDATE dbo.Questions
SET dbo.Questions.CalendarID = a.CalendarID
FROM dbo.Questions
INNER JOIN 
(
SELECT QuestionID, MAX(CalendarID) as CalendarID FROM dbo.Answers
GROUP BY QuestionID
) a on dbo.Questions.Id = a.QuestionID
");

            DropColumn("dbo.Calendars", "QuestionId");
            DropColumn("dbo.Answers", "CalendarID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Answers", "CalendarID", c => c.Int());
            AddColumn("dbo.Calendars", "QuestionId", c => c.Int());
            DropForeignKey("dbo.Questions", "CalendarID", "dbo.Calendars");
            DropIndex("dbo.Questions", new[] { "CalendarID" });
            DropColumn("dbo.Questions", "CalendarID");
            CreateIndex("dbo.Answers", "CalendarID");
            CreateIndex("dbo.Calendars", "QuestionId");
            AddForeignKey("dbo.Calendars", "QuestionId", "dbo.Questions", "Id");
            AddForeignKey("dbo.Answers", "CalendarID", "dbo.Calendars", "Id");
        }
    }
}
