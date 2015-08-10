namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAuthorizationTokens : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AuthorizationTokens",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        UserID = c.String(maxLength: 128),
                        RedirectURL = c.String(),
                        ExpiresAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserID)
                .Index(t => t.UserID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AuthorizationTokens", "UserID", "dbo.Users");
            DropIndex("dbo.AuthorizationTokens", new[] { "UserID" });
            DropTable("dbo.AuthorizationTokens");
        }
    }
}
