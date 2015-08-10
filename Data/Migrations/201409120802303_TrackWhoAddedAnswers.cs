namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrackWhoAddedAnswers : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.Answers", name: "User_Id", newName: "UserID");
            RenameIndex(table: "dbo.Answers", name: "IX_User_Id", newName: "IX_UserID");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Answers", name: "IX_UserID", newName: "IX_User_Id");
            RenameColumn(table: "dbo.Answers", name: "UserID", newName: "User_Id");
        }
    }
}
