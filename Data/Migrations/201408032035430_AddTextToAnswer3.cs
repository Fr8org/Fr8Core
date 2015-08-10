namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddTextToAnswer3 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Answers", new[] { "User_Id" });
            AlterColumn("dbo.Answers", "User_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.Answers", "User_Id");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Answers", new[] { "User_Id" });
            AlterColumn("dbo.Answers", "User_Id", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Answers", "User_Id");
        }
    }
}
