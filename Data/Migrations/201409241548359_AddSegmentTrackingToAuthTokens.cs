namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddSegmentTrackingToAuthTokens : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuthorizationTokens", "SegmentTrackingEventName", c => c.String());
            AddColumn("dbo.AuthorizationTokens", "SegmentTrackingProperties", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuthorizationTokens", "SegmentTrackingProperties");
            DropColumn("dbo.AuthorizationTokens", "SegmentTrackingEventName");
        }
    }
}
