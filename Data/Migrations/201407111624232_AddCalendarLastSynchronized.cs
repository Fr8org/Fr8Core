namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCalendarLastSynchronized : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Calendars", "LastSynchronized", c => c.DateTimeOffset(nullable: false, precision: 7, defaultValue: DateTimeOffset.MinValue));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Calendars", "LastSynchronized");
        }
    }
}
