namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BookerAvailability : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Available", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Available");
        }
    }
}
