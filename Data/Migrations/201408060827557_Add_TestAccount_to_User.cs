namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Add_TestAccount_to_User : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "TestAccount", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "TestAccount");
        }
    }
}
