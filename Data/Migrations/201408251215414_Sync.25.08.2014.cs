namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Sync25082014 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ClarificationRequests", "Id", "dbo.Emails");
            DropForeignKey("dbo.ClarificationRequests", "ClarificationRequestState", "dbo._ClarificationRequestStateTemplate");
            DropForeignKey("dbo.ClarificationRequests", "NegotiationId", "dbo.Negotiations");
            DropIndex("dbo.ClarificationRequests", new[] { "Id" });
            DropIndex("dbo.ClarificationRequests", new[] { "ClarificationRequestState" });
            DropIndex("dbo.ClarificationRequests", new[] { "NegotiationId" });

            DropIndex("dbo.Questions", new[] { "ClarificationRequestID" });
            DropForeignKey("dbo.Questions", "ClarificationRequestID", "dbo.ClarificationRequests");
            DropColumn("dbo.Questions", "ClarificationRequestID");

            DropTable("dbo.ClarificationRequests");

            Sql(@"DROP TABLE dbo._ClarificationRequestStateTemplate");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ClarificationRequests",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ClarificationRequestState = c.Int(nullable: false),
                        NegotiationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo._ClarificationRequestStateTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.ClarificationRequests", "NegotiationId");
            CreateIndex("dbo.ClarificationRequests", "ClarificationRequestState");
            CreateIndex("dbo.ClarificationRequests", "Id");
            AddForeignKey("dbo.ClarificationRequests", "NegotiationId", "dbo.Negotiations", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ClarificationRequests", "ClarificationRequestState", "dbo._ClarificationRequestStateTemplate", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ClarificationRequests", "Id", "dbo.Emails", "Id");
        }
    }
}
