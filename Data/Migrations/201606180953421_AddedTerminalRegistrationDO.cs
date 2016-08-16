namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedTerminalRegistrationDO : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TerminalRegistration",
                c => new
                    {
                        Endpoint = c.String(nullable: false, maxLength: 256),
                        UserId = c.String(maxLength: 128),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Endpoint)
                .ForeignKey("dbo.Users", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TerminalRegistration", "UserId", "dbo.Users");
            DropIndex("dbo.TerminalRegistration", new[] { "UserId" });
            DropTable("dbo.TerminalRegistration");
        }
    }
}
