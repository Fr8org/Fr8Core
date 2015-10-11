namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AuthorizationTokenDO_ExternalStateToken_Migration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuthorizationTokens", "ExternalStateToken", c => c.String(nullable: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuthorizationTokens", "ExternalStateToken");
        }
    }
}
