namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateActionDo : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Actions", new[] { "ActionListID" });
            AddColumn("dbo.Actions", "UserLabel", c => c.String());
            AddColumn("dbo.Actions", "ActionType", c => c.String());
            AddColumn("dbo.Actions", "ConfigurationSettings", c => c.String());
            AddColumn("dbo.Actions", "FieldMappingSettings", c => c.String());
            AddColumn("dbo.Actions", "ParentPluginRegistration", c => c.String());
            CreateIndex("dbo.Actions", "ActionListId");
            DropColumn("dbo.Actions", "Name");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Actions", "Name", c => c.String());
            DropIndex("dbo.Actions", new[] { "ActionListId" });
            DropColumn("dbo.Actions", "ParentPluginRegistration");
            DropColumn("dbo.Actions", "FieldMappingSettings");
            DropColumn("dbo.Actions", "ConfigurationSettings");
            DropColumn("dbo.Actions", "ActionType");
            DropColumn("dbo.Actions", "UserLabel");
            CreateIndex("dbo.Actions", "ActionListID");
        }
    }
}
