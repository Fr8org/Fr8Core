namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Authorization_V2_Migration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Actions", "AuthorizationTokenId", c => c.Guid());
            AddColumn("dbo.AuthorizationTokens", "IsMain", c => c.Boolean(nullable: true));

            Sql("UPDATE [dbo].[AuthorizationTokens] SET [IsMain] = 0");
            AlterColumn("dbo.AuthorizationTokens", "IsMain", c => c.Boolean(nullable: false));

            CreateIndex("dbo.Actions", "AuthorizationTokenId");
            AddForeignKey("dbo.Actions", "AuthorizationTokenId", "dbo.AuthorizationTokens", "Id");
        }
        
        public override void Down()
        {   
            DropForeignKey("dbo.Actions", "AuthorizationTokenId", "dbo.AuthorizationTokens");
            DropIndex("dbo.Actions", new[] { "AuthorizationTokenId" });
            DropColumn("dbo.AuthorizationTokens", "IsMain");
            DropColumn("dbo.Actions", "AuthorizationTokenId");
        }
    }
}
