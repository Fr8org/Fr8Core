namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateAuthorizationToken : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AuthorizationTokenSecureDataLocal",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Data = c.String(),
                        LastUpdated = c.DateTimeOffset(nullable: false, precision: 7),
                        CreateDate = c.DateTimeOffset(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            DropColumn("dbo.AuthorizationTokens", "Token");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AuthorizationTokens", "Token", c => c.String());
            DropTable("dbo.AuthorizationTokenSecureDataLocal");
        }
    }
}
