namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixCascadeForAnswerResponse : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.QuestionResponses", "AnswerID", "dbo.Answers");
            DropIndex("dbo.QuestionResponses", new[] { "AnswerID" });
            DropIndex("dbo.QuestionResponses", new[] { "UserID" });

            Sql(@"DELETE FROM dbo.QuestionResponses WHERE AnswerID IS NULL OR UserID IS NULL");

            AlterColumn("dbo.QuestionResponses", "AnswerID", c => c.Int(nullable: false));
            AlterColumn("dbo.QuestionResponses", "UserID", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.QuestionResponses", "AnswerID");
            CreateIndex("dbo.QuestionResponses", "UserID");
            AddForeignKey("dbo.QuestionResponses", "AnswerID", "dbo.Answers", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuestionResponses", "AnswerID", "dbo.Answers");
            DropIndex("dbo.QuestionResponses", new[] { "UserID" });
            DropIndex("dbo.QuestionResponses", new[] { "AnswerID" });
            AlterColumn("dbo.QuestionResponses", "UserID", c => c.String(maxLength: 128));
            AlterColumn("dbo.QuestionResponses", "AnswerID", c => c.Int());
            CreateIndex("dbo.QuestionResponses", "UserID");
            CreateIndex("dbo.QuestionResponses", "AnswerID");
            AddForeignKey("dbo.QuestionResponses", "AnswerID", "dbo.Answers", "Id");
        }
    }
}
