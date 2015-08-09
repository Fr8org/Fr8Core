namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class FixTypeOfKey : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Attendees", "TempColumn", c => c.Int());
            Sql("Update dbo.Attendees SET TempColumn = EventID");

            DropForeignKey("dbo.Attendees", "EventID", "dbo.Events");
            DropIndex("dbo.Attendees", new[] { "EventID" });
            AlterColumn("dbo.Attendees", "EventID", c => c.Int());
            CreateIndex("dbo.Attendees", "EventID");
            AddForeignKey("dbo.Attendees", "EventID", "dbo.Events", "Id");

            Sql("Update dbo.Attendees SET EventID = TempColumn");
            DropColumn("dbo.Attendees", "TempColumn");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Attendees", "TempColumn", c => c.Int());
            Sql("Update dbo.Attendees SET TempColumn = EventID");

            DropForeignKey("dbo.Attendees", "EventID", "dbo.Events");
            DropIndex("dbo.Attendees", new[] { "EventID" });
            AlterColumn("dbo.Attendees", "EventID", c => c.Int(nullable: false));
            CreateIndex("dbo.Attendees", "EventID");
            AddForeignKey("dbo.Attendees", "EventID", "dbo.Events", "Id", cascadeDelete: true);

            Sql("Update dbo.Attendees SET EventID = TempColumn");
            DropColumn("dbo.Attendees", "TempColumn");
        }
    }
}
