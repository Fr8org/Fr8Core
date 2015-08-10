namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExpectedResponses : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExpectedResponses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AssociatedObjectType = c.String(),
                        AssociatedObjectID = c.Int(nullable: false),
                        UserID = c.String(maxLength: 128),
                        Status = c.Int(nullable: false),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo._ExpectedResponseStatusTemplate", t => t.Status, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID)
                .Index(t => t.Status);
            
            CreateTable(
                "dbo._ExpectedResponseStatusTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExpectedResponses", "UserID", "dbo.Users");
            DropForeignKey("dbo.ExpectedResponses", "Status", "dbo._ExpectedResponseStatusTemplate");
            DropIndex("dbo.ExpectedResponses", new[] { "Status" });
            DropIndex("dbo.ExpectedResponses", new[] { "UserID" });
            DropTable("dbo._ExpectedResponseStatusTemplate");
            DropTable("dbo.ExpectedResponses");
        }
    }
}
