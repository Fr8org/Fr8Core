namespace Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class RenamedEnums : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.BookingRequestStatuses", newName: "BookingRequestStates");
            CreateTable(
                "dbo.ClarificationRequestStates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ClarificationRequests", "CRState", c => c.Int(nullable: false));
            Sql("UPDATE dbo.ClarificationRequests SET CRState = ClarificationStatus");
            CreateIndex("dbo.ClarificationRequests", "CRState");
            AddForeignKey("dbo.ClarificationRequests", "CRState", "dbo.BookingRequestStates", "Id", cascadeDelete: true);
            DropColumn("dbo.ClarificationRequests", "ClarificationStatus");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ClarificationRequests", "ClarificationStatus", c => c.Int(nullable: false));
            Sql("UPDATE dbo.ClarificationRequests SET ClarificationStatus = CRState");
            DropForeignKey("dbo.ClarificationRequests", "CRState", "dbo.BookingRequestStates");
            DropIndex("dbo.ClarificationRequests", new[] { "CRState" });
            DropColumn("dbo.ClarificationRequests", "CRState");
            DropTable("dbo.ClarificationRequestStates");
            RenameTable(name: "dbo.BookingRequestStates", newName: "BookingRequestStatuses");
        }
    }
}
