namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Activity_ClientVisibilityAllowNULL : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ActivityTemplate", "ClientVisibility", c => c.Boolean());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ActivityTemplate", "ClientVisibility", c => c.Boolean(nullable: false));
        }
    }
}
