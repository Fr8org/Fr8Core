namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAuthorizationTokenDOTokenProperty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuthorizationTokens", "Token", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuthorizationTokens", "Token");
        }
    }
}
