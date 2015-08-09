namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SyncRob16102014 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Attendees", "ParticipationStatus", "dbo._ParticipationStatusTemplate");
            DropIndex("dbo.Attendees", new[] { "ParticipationStatus" });
            AlterColumn("dbo.Attendees", "ParticipationStatus", c => c.Int());
            CreateIndex("dbo.Attendees", "ParticipationStatus");
            AddForeignKey("dbo.Attendees", "ParticipationStatus", "dbo._ParticipationStatusTemplate", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Attendees", "ParticipationStatus", "dbo._ParticipationStatusTemplate");
            DropIndex("dbo.Attendees", new[] { "ParticipationStatus" });
            AlterColumn("dbo.Attendees", "ParticipationStatus", c => c.Int(nullable: false));
            CreateIndex("dbo.Attendees", "ParticipationStatus");
            AddForeignKey("dbo.Attendees", "ParticipationStatus", "dbo._ParticipationStatusTemplate", "Id", cascadeDelete: true);
        }
    }
}
