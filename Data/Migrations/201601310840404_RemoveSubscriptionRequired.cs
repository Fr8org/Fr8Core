namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveSubscriptionRequired : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Terminals", "SubscriptionRequired");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Terminals", "SubscriptionRequired", c => c.Boolean(nullable: false));
        }
    }
}
