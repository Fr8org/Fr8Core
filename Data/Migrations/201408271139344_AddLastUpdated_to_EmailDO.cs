namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLastUpdated_to_EmailDO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Emails", "LastUpdated", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Emails", "LastUpdated");
        }
    }
}
