namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addExternalAccountId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuthorizationTokens", "ExternalAccountId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuthorizationTokens", "ExternalAccountId");
        }
    }
}
