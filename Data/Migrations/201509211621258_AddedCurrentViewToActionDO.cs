namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedCurrentViewToActionDO : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Actions", "currentView", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Actions", "currentView");
        }
    }
}
