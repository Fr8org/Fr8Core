namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveParentPluginRegistration : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Actions", "ParentPluginRegistration");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Actions", "ParentPluginRegistration", c => c.String());
        }
    }
}
