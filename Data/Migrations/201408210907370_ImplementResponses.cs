namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class ImplementResponses : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuestionResponses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuestionID = c.Int(nullable: false),
                        AnswerID = c.Int(),
                        Text = c.String(),
                        CalendarID = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Answers", t => t.AnswerID)
                .ForeignKey("dbo.Calendars", t => t.CalendarID)
                .ForeignKey("dbo.Questions", t => t.QuestionID, cascadeDelete: true)
                .Index(t => t.QuestionID)
                .Index(t => t.AnswerID)
                .Index(t => t.CalendarID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuestionResponses", "QuestionID", "dbo.Questions");
            DropForeignKey("dbo.QuestionResponses", "CalendarID", "dbo.Calendars");
            DropForeignKey("dbo.QuestionResponses", "AnswerID", "dbo.Answers");
            DropIndex("dbo.QuestionResponses", new[] { "CalendarID" });
            DropIndex("dbo.QuestionResponses", new[] { "AnswerID" });
            DropIndex("dbo.QuestionResponses", new[] { "QuestionID" });
            DropTable("dbo.QuestionResponses");
        }
    }
}
