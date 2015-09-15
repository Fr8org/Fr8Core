namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ActivityTemplate", newName: "ActionTemplate");
            RenameColumn(table: "dbo.Actions", name: "ActivityTemplateId", newName: "ActionTemplateId");
            RenameIndex(table: "dbo.Actions", name: "IX_ActivityTemplateId", newName: "IX_ActionTemplateId");
            DropColumn("dbo.ActionTemplate", "ComponentActivities");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ActionTemplate", "ComponentActivities", c => c.String());
            RenameIndex(table: "dbo.Actions", name: "IX_ActionTemplateId", newName: "IX_ActivityTemplateId");
            RenameColumn(table: "dbo.Actions", name: "ActionTemplateId", newName: "ActivityTemplateId");
            RenameTable(name: "dbo.ActionTemplate", newName: "ActivityTemplate");
        }
    }
}
