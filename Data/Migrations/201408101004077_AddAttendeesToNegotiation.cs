namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddAttendeesToNegotiation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Attendees", "NegotiationID", c => c.Int());
            CreateIndex("dbo.Attendees", "NegotiationID");
            AddForeignKey("dbo.Attendees", "NegotiationID", "dbo.Negotiations", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Attendees", "NegotiationID", "dbo.Negotiations");
            DropIndex("dbo.Attendees", new[] { "NegotiationID" });
            DropColumn("dbo.Attendees", "NegotiationID");
        }
    }
}
