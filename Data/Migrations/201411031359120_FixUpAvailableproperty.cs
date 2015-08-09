namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixUpAvailableproperty : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Users", "Available");
            AddColumn("dbo.Users", "Available", c => c.Boolean(nullable: false, defaultValue: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Available");
            AddColumn("dbo.Users", "Available", c => c.Boolean(nullable: false));
        }
    }
}
