namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedNameAndCurrentViewFromActivityDO : System.Data.Entity.Migrations.DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Actions", "Name");
            DropColumn("dbo.Actions", "currentView");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Actions", "currentView", c => c.String());
            AddColumn("dbo.Actions", "Name", c => c.String());
        }
    }
}
