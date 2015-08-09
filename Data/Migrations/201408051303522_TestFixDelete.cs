namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class TestFixDelete : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Attendees", "EventID", "dbo.Events");
            AddForeignKey("dbo.Attendees", "EventID", "dbo.Events", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Attendees", "EventID", "dbo.Events");
            AddForeignKey("dbo.Attendees", "EventID", "dbo.Events", "Id", cascadeDelete: false);
        }
    }
}
