namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModifiedAuthorizeTokenTable : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuthorizationTokens", "AdditionalAttributes", c => c.String());          
        }
        
        public override void Down()
        {        
            DropColumn("dbo.AuthorizationTokens", "AdditionalAttributes");
        }
    }
}
