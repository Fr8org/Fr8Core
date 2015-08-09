namespace Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddCalendars : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "CalendarID", c => c.Int(nullable: false));
            DropColumn("dbo.Calendars", "PersonId");

            Sql(
@"MERGE [dbo].[Calendars] [c]
    USING [dbo].[Users] [u]
    ON ([u].[Id] = [c].[OwnerID])
WHEN NOT MATCHED THEN
    INSERT ([Name], [OwnerID])
    VALUES ('Default Calendar', [u].[Id]);");

            Sql(
@"UPDATE [e]
SET [e].[CalendarID] = [c].[Id]
FROM [dbo].[Events] AS [e]
INNER JOIN [dbo].[BookingRequests] AS [br] ON [e].[BookingRequestID] = [br].[Id]
INNER JOIN [dbo].[Calendars] AS [c] ON [br].[User_Id] = [c].[OwnerID]");

            CreateIndex("dbo.Events", "CalendarID");
            AddForeignKey("dbo.Events", "CalendarID", "dbo.Calendars", "Id", cascadeDelete: true);
        }

        public override void Down()
        {
            AddColumn("dbo.Calendars", "PersonId", c => c.Int(nullable: false));
            DropForeignKey("dbo.Events", "CalendarID", "dbo.Calendars");
            DropIndex("dbo.Events", new[] { "CalendarID" });
            DropColumn("dbo.Events", "CalendarID");
        }
    }
}
