namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ActivityClientVisibility : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActivityTemplate", "ClientVisibility", c => c.Boolean(nullable: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ActivityTemplate", "ClientVisibility");
        }
    }
}
