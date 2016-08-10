namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveSubscriptionRequiredAndNameFromActivity : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Terminals", "SubscriptionRequired");
            DropColumn("dbo.Actions", "Name");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Actions", "Name", c => c.String());
            AddColumn("dbo.Terminals", "SubscriptionRequired", c => c.Boolean(nullable: false));
        }
    }
}
