namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AttendeeNegotiationRelationship : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Attendees", "Id");
            AddForeignKey("dbo.Attendees", "Id", "dbo.Negotiations", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Attendees", "Id", "dbo.Negotiations");
            DropIndex("dbo.Attendees", new[] { "Id" });
        }
    }
}
