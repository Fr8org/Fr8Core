namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class LinkBRToNegotiation : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Calendars", "ClarificationRequestID", "dbo.ClarificationRequests");
            DropIndex("dbo.Calendars", new[] { "ClarificationRequestID" });
            
            CreateTable(
                "dbo.NegotiationStateRows",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AnswerStatusRows",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Calendars", "NegotiationID", c => c.Int());            
            CreateIndex("dbo.Calendars", "NegotiationID");
            AddForeignKey("dbo.Calendars", "NegotiationID", "dbo.Negotiations", "Id");
            DropColumn("dbo.Calendars", "ClarificationRequestID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Calendars", "ClarificationRequestID", c => c.Int());
            DropForeignKey("dbo.ClarificationRequests", "NegotiationId", "dbo.Negotiations");
            DropForeignKey("dbo.Answers", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Answers", "QuestionID", "dbo.Questions");
            DropForeignKey("dbo.Answers", "AnswerStatusID", "dbo.AnswerStatusRows");
            DropForeignKey("dbo.Negotiations", "BookingRequestID", "dbo.BookingRequests");
            DropForeignKey("dbo.Questions", "NegotiationId", "dbo.Negotiations");
            DropForeignKey("dbo.Negotiations", "NegotiationStateID", "dbo.NegotiationStateRows");
            DropForeignKey("dbo.Negotiations", "RequestId", "dbo.Emails");
            DropForeignKey("dbo.Calendars", "NegotiationID", "dbo.Negotiations");
            DropIndex("dbo.ClarificationRequests", new[] { "NegotiationId" });
            DropIndex("dbo.Answers", new[] { "User_Id" });
            DropIndex("dbo.Answers", new[] { "AnswerStatusID" });
            DropIndex("dbo.Answers", new[] { "QuestionID" });
            DropIndex("dbo.Questions", new[] { "NegotiationId" });
            DropIndex("dbo.Negotiations", new[] { "RequestId" });
            DropIndex("dbo.Negotiations", new[] { "BookingRequestID" });
            DropIndex("dbo.Negotiations", new[] { "NegotiationStateID" });
            DropIndex("dbo.Calendars", new[] { "NegotiationID" });
            DropColumn("dbo.Questions", "NegotiationId");
            DropColumn("dbo.Questions", "AnswerType");
            DropColumn("dbo.ClarificationRequests", "NegotiationId");
            DropColumn("dbo.Calendars", "NegotiationID");
            DropTable("dbo.AnswerStatusRows");
            DropTable("dbo.Answers");
            DropTable("dbo.NegotiationStateRows");
            DropTable("dbo.Negotiations");
            CreateIndex("dbo.Calendars", "ClarificationRequestID");
            AddForeignKey("dbo.Calendars", "ClarificationRequestID", "dbo.ClarificationRequests", "Id");
        }
    }
}
