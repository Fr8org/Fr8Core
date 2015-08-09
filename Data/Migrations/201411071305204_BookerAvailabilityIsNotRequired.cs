namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BookerAvailabilityIsNotRequired : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Users", "Available", c => c.Boolean());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Users", "Available", c => c.Boolean(nullable: false));
        }
    }
}
