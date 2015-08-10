namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class ParticipantStatusIsRequired : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Attendees", "ParticipationStatus", "dbo._ParticipationStatusTemplate");
            DropIndex("dbo.Attendees", new[] { "ParticipationStatus" });

            Sql(@"UPDATE [dbo].[Attendees] SET ParticipationStatus = 1 WHERE ParticipationStatus = 0");

            AlterColumn("dbo.Attendees", "ParticipationStatus", c => c.Int(nullable: false));
            CreateIndex("dbo.Attendees", "ParticipationStatus");
            AddForeignKey("dbo.Attendees", "ParticipationStatus", "dbo._ParticipationStatusTemplate", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Attendees", "ParticipationStatus", "dbo._ParticipationStatusTemplate");
            DropIndex("dbo.Attendees", new[] { "ParticipationStatus" });
            AlterColumn("dbo.Attendees", "ParticipationStatus", c => c.Int());
            CreateIndex("dbo.Attendees", "ParticipationStatus");
            AddForeignKey("dbo.Attendees", "ParticipationStatus", "dbo._ParticipationStatusTemplate", "Id");
        }
    }
}
