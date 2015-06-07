namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFieldToTrackTimezonesForUsers : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "TimeZoneID", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "TimeZoneID");
        }
    }
}
