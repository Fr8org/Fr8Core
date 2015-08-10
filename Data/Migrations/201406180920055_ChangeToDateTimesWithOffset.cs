namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class ChangeToDateTimesWithOffset : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Emails", "DateReceived");
            DropColumn("dbo.Emails", "DateCreated");
            DropColumn("dbo.Events", "StartDate");
            DropColumn("dbo.Events", "EndDate");

            AddColumn("dbo.Emails", "DateReceived", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Emails", "DateCreated", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Events", "StartDate", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Events", "EndDate", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Emails", "DateReceived");
            DropColumn("dbo.Emails", "DateCreated");
            DropColumn("dbo.Events", "StartDate");
            DropColumn("dbo.Events", "EndDate");

            AddColumn("dbo.Events", "EndDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Events", "StartDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Emails", "DateCreated", c => c.DateTime(nullable: false));
            AddColumn("dbo.Emails", "DateReceived", c => c.DateTime(nullable: false));
        }
    }
}
