namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Test_migration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Terminals", "SubscriptionRequired", c => c.Boolean(nullable: false));
            AddColumn("dbo.Terminals", "UserDO_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.Terminals", "UserDO_Id");
            AddForeignKey("dbo.Terminals", "UserDO_Id", "dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Terminals", "UserDO_Id", "dbo.Users");
            DropIndex("dbo.Terminals", new[] { "UserDO_Id" });
            DropColumn("dbo.Terminals", "UserDO_Id");
            DropColumn("dbo.Terminals", "SubscriptionRequired");
        }
    }
}
