namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateActionDo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Actions", "ConfigurationSettings", c => c.String());
            AddColumn("dbo.Actions", "FieldMappingSettings", c => c.String());
            AddColumn("dbo.Actions", "ParentPluginRegistration", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Actions", "ParentPluginRegistration");
            DropColumn("dbo.Actions", "FieldMappingSettings");
            DropColumn("dbo.Actions", "ConfigurationSettings");
        }
    }
}
