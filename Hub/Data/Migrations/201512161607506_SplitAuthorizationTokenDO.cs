namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SplitAuthorizationTokenDO : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EncryptedAuthorizationData",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Data = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropColumn("dbo.AuthorizationTokens", "Token");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AuthorizationTokens", "Token", c => c.String());
            DropTable("dbo.EncryptedAuthorizationData");
        }
    }
}
