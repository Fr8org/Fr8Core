namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserClaims_BaseColumns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUserClaims", "LastUpdated", c => c.DateTimeOffset(precision: 7));
            AddColumn("dbo.AspNetUserClaims", "CreateDate", c => c.DateTimeOffset(precision: 7));
            AddColumn("dbo.AspNetUserClaims", "Discriminator", c => c.String(nullable: false, maxLength: 128));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUserClaims", "Discriminator");
            DropColumn("dbo.AspNetUserClaims", "CreateDate");
            DropColumn("dbo.AspNetUserClaims", "LastUpdated");
        }
    }
}
