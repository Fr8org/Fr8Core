namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddUserGoogleAuthData : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "GoogleAuthData", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "GoogleAuthData");
        }
    }
}
