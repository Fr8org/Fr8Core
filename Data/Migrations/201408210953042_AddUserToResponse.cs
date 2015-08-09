namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddUserToResponse : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuestionResponses", "UserID", c => c.String(maxLength: 128));
            CreateIndex("dbo.QuestionResponses", "UserID");
            AddForeignKey("dbo.QuestionResponses", "UserID", "dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuestionResponses", "UserID", "dbo.Users");
            DropIndex("dbo.QuestionResponses", new[] { "UserID" });
            DropColumn("dbo.QuestionResponses", "UserID");
        }
    }
}
