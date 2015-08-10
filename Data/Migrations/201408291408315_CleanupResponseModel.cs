namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CleanupResponseModel : DbMigration
    {
        public override void Up()
        {
            Sql(@"DELETE FROM dbo.QuestionResponses WHERE AnswerID IS NULL");

            DropForeignKey("dbo.QuestionResponses", "CalendarID", "dbo.Calendars");
            DropForeignKey("dbo.QuestionResponses", "QuestionID", "dbo.Questions");
            DropForeignKey("dbo.QuestionResponses", "AnswerID", "dbo.Answers");
            DropIndex("dbo.QuestionResponses", new[] { "QuestionID" });
            DropIndex("dbo.QuestionResponses", new[] { "AnswerID" });
            DropIndex("dbo.QuestionResponses", new[] { "CalendarID" });
            AlterColumn("dbo.QuestionResponses", "AnswerID", c => c.Int(nullable: false));
            CreateIndex("dbo.QuestionResponses", "AnswerID");
            AddForeignKey("dbo.QuestionResponses", "AnswerID", "dbo.Answers", "Id", cascadeDelete: true);
            DropColumn("dbo.QuestionResponses", "QuestionID");
            DropColumn("dbo.QuestionResponses", "Text");
            DropColumn("dbo.QuestionResponses", "CalendarID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuestionResponses", "CalendarID", c => c.Int());
            AddColumn("dbo.QuestionResponses", "Text", c => c.String());
            AddColumn("dbo.QuestionResponses", "QuestionID", c => c.Int(nullable: false));
            DropForeignKey("dbo.QuestionResponses", "AnswerID", "dbo.Answers");
            DropIndex("dbo.QuestionResponses", new[] { "AnswerID" });
            AlterColumn("dbo.QuestionResponses", "AnswerID", c => c.Int());
            CreateIndex("dbo.QuestionResponses", "CalendarID");
            CreateIndex("dbo.QuestionResponses", "AnswerID");
            CreateIndex("dbo.QuestionResponses", "QuestionID");
            AddForeignKey("dbo.QuestionResponses", "AnswerID", "dbo.Answers", "Id");
            AddForeignKey("dbo.QuestionResponses", "QuestionID", "dbo.Questions", "Id", cascadeDelete: true);
            AddForeignKey("dbo.QuestionResponses", "CalendarID", "dbo.Calendars", "Id");
        }
    }
}
