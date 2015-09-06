namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameActionTemplateFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActionTemplate", "Name", c => c.String());
            AddColumn("dbo.ActionTemplate", "DefaultEndPoint", c => c.String());
            DropColumn("dbo.ActionTemplate", "ActionType");
            DropColumn("dbo.ActionTemplate", "ParentPluginRegistration");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ActionTemplate", "ParentPluginRegistration", c => c.String());
            AddColumn("dbo.ActionTemplate", "ActionType", c => c.String());
            DropColumn("dbo.ActionTemplate", "DefaultEndPoint");
            DropColumn("dbo.ActionTemplate", "Name");
        }
    }
}
