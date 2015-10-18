namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AuthorizeTokenRepositaryUpdateWithSalesforceSpecificColumns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuthorizationTokens", "InstanceUrl", c => c.String());
            AddColumn("dbo.AuthorizationTokens", "ApiVersion", c => c.String());
            AddColumn("dbo.AuthorizationTokens", "RefreshToken", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuthorizationTokens", "RefreshToken");
            DropColumn("dbo.AuthorizationTokens", "ApiVersion");
            DropColumn("dbo.AuthorizationTokens", "InstanceUrl");
        }
    }
}
