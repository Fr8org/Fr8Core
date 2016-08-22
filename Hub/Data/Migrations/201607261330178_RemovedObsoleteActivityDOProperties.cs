namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedObsoleteActivityDOProperties : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ActivityTemplate", "ComponentActivities");
            DropColumn("dbo.ActivityTemplate", "ClientVisibility");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ActivityTemplate", "ClientVisibility", c => c.Boolean());
            AddColumn("dbo.ActivityTemplate", "ComponentActivities", c => c.String());
        }
    }
}
