namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExternalAccountAndDomainNameToAuthToken : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuthorizationTokens", "ExternalAccountName", c => c.String());
            AddColumn("dbo.AuthorizationTokens", "ExternalDomainName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuthorizationTokens", "ExternalDomainName");
            DropColumn("dbo.AuthorizationTokens", "ExternalAccountName");
        }
    }
}
