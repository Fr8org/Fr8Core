namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenamedTableActionTemplatetoActivityTemplate : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ActionTemplate", newName: "ActivityTemplate");
            RenameColumn(table: "dbo.Actions", name: "ActionTemplateId", newName: "ActivityTemplateId");
            RenameIndex(table: "dbo.Actions", name: "IX_ActionTemplateId", newName: "IX_ActivityTemplateId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Actions", name: "IX_ActivityTemplateId", newName: "IX_ActionTemplateId");
            RenameColumn(table: "dbo.Actions", name: "ActivityTemplateId", newName: "ActionTemplateId");
            RenameTable(name: "dbo.ActivityTemplate", newName: "ActionTemplate");
        }
    }
}
