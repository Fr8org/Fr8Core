namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddNegotiationAnswerEmail : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NegotiationAnswerEmails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EmailID = c.Int(nullable: false),
                        NegotiationID = c.Int(nullable: false),
                        UserID = c.String(nullable: false, maxLength: 128),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Emails", t => t.EmailID, cascadeDelete: true)
                .ForeignKey("dbo.Negotiations", t => t.NegotiationID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.EmailID)
                .Index(t => t.NegotiationID)
                .Index(t => t.UserID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NegotiationAnswerEmails", "UserID", "dbo.Users");
            DropForeignKey("dbo.NegotiationAnswerEmails", "NegotiationID", "dbo.Negotiations");
            DropForeignKey("dbo.NegotiationAnswerEmails", "EmailID", "dbo.Emails");
            DropIndex("dbo.NegotiationAnswerEmails", new[] { "UserID" });
            DropIndex("dbo.NegotiationAnswerEmails", new[] { "NegotiationID" });
            DropIndex("dbo.NegotiationAnswerEmails", new[] { "EmailID" });
            DropTable("dbo.NegotiationAnswerEmails");
        }
    }
}
