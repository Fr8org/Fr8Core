namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class RemoveRequestIDFromNegotiationDO : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Negotiations", "RequestId", "dbo.Emails");
            DropIndex("dbo.Negotiations", new[] { "RequestId" });
            DropColumn("dbo.Negotiations", "RequestId");

            AddColumn("dbo.Calendars", "QuestionID", c => c.Int(nullable: true));
            CreateIndex("dbo.Calendars", "QuestionID");
            AddForeignKey("dbo.Calendars", "QuestionID", "dbo.Questions", "Id", cascadeDelete: true);

        }
        
        public override void Down()
        {
            AddColumn("dbo.Negotiations", "RequestId", c => c.Int(nullable: false));
            CreateIndex("dbo.Negotiations", "RequestId");
            AddForeignKey("dbo.Negotiations", "RequestId", "dbo.Emails", "Id", cascadeDelete: true);

            DropForeignKey("dbo.Calendars", "QuestionID", "dbo.Questions");
            DropIndex("dbo.Negotiations", new[] { "QuestionID" });
            DropColumn("dbo.Calendars", "QuestionID");
        }
    }
}
