namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixLastUpdated : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetRoles", "LastUpdated");
            DropColumn("dbo.AspNetUserRoles", "LastUpdated");

            AddColumn("dbo.AspNetRoles", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.AspNetUserRoles", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetRoles", "LastUpdated");
            DropColumn("dbo.AspNetUserRoles", "LastUpdated");

            AddColumn("dbo.AspNetRoles", "LastUpdated", c => c.DateTimeOffset(precision: 7));
            AddColumn("dbo.AspNetUserRoles", "LastUpdated", c => c.DateTimeOffset(precision: 7));
        }
    }
}
