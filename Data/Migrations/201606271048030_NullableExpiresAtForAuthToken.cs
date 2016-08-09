namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NullableExpiresAtForAuthToken : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AuthorizationTokens", "ExpiresAt", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AuthorizationTokens", "ExpiresAt", c => c.DateTime(nullable: false));
        }
    }
}
