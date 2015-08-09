namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddNegotiations : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Negotiations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        State = c.Int(nullable: false),
                        Name = c.String(),
                        RequestId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Emails", t => t.RequestId, cascadeDelete: true)
                .Index(t => t.RequestId);

            CreateTable(
                "dbo.Answers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuestionID = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        ObjectsType = c.String(),
                        User_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Questions", t => t.QuestionID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.QuestionID)
                .Index(t => t.User_Id);
            
            //The schema says Questions and ClarificationRequests MUST have a negotiation, so the existing rows are invalid; delete them.
            Sql(@"DELETE FROM dbo.Questions");
            Sql(@"DELETE FROM dbo.ClarificationRequests");
            
            AddColumn("dbo.ClarificationRequests", "NegotiationId", c => c.Int(nullable: false));
            AddColumn("dbo.Questions", "AnswerType", c => c.String());
            AddColumn("dbo.Questions", "NegotiationId", c => c.Int(nullable: false));
            CreateIndex("dbo.Questions", "NegotiationId");
            CreateIndex("dbo.ClarificationRequests", "NegotiationId");
            AddForeignKey("dbo.Questions", "NegotiationId", "dbo.Negotiations", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ClarificationRequests", "NegotiationId", "dbo.Negotiations", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            //DropForeignKey("dbo.ClarificationRequests", "NegotiationId", "dbo.Negotiations");
            //DropForeignKey("dbo.Answers", "User_Id", "dbo.Users");
            //DropForeignKey("dbo.Answers", "QuestionID", "dbo.Questions");
            //DropForeignKey("dbo.Questions", "NegotiationId", "dbo.Negotiations");
            //DropForeignKey("dbo.Negotiations", "RequestId", "dbo.Emails");
            //DropIndex("dbo.ClarificationRequests", new[] { "NegotiationId" });
            //DropIndex("dbo.Answers", new[] { "User_Id" });
            //DropIndex("dbo.Answers", new[] { "QuestionID" });
            //DropIndex("dbo.Questions", new[] { "NegotiationId" });
            //DropIndex("dbo.Negotiations", new[] { "RequestId" });
            //DropColumn("dbo.Questions", "NegotiationId");
            //DropColumn("dbo.Questions", "AnswerType");
            //DropColumn("dbo.ClarificationRequests", "NegotiationId");
            //DropTable("dbo.Answers");
            //DropTable("dbo.Negotiations");
        }
    }
}
