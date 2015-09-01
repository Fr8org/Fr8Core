namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DO_849_Migration : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ExternalEventSubscriptions", "ExternalEvent", "dbo._EventStatusTemplate");
            DropForeignKey("dbo.Actions", "ActionTemplateId", "dbo.ActionTemplate");
            DropIndex("dbo.ExternalEventSubscriptions", new[] { "ExternalEvent" });
            DropIndex("dbo.Actions", new[] { "ActionTemplateId" });
            AlterColumn("dbo.Actions", "ActionTemplateId", c => c.Int());
            AlterColumn("dbo.ExternalEventSubscriptions", "ExternalEvent", c => c.Int());
            CreateIndex("dbo.ExternalEventSubscriptions", "ExternalEvent");
            CreateIndex("dbo.Actions", "ActionTemplateId");
            AddForeignKey("dbo.ExternalEventSubscriptions", "ExternalEvent", "dbo._EventStatusTemplate", "Id");
            AddForeignKey("dbo.Actions", "ActionTemplateId", "dbo.ActionTemplate", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Actions", "ActionTemplateId", "dbo.ActionTemplate");
            DropForeignKey("dbo.ExternalEventSubscriptions", "ExternalEvent", "dbo._EventStatusTemplate");
            DropIndex("dbo.Actions", new[] { "ActionTemplateId" });
            DropIndex("dbo.ExternalEventSubscriptions", new[] { "ExternalEvent" });
            AlterColumn("dbo.ExternalEventSubscriptions", "ExternalEvent", c => c.Int(nullable: false));
            AlterColumn("dbo.Actions", "ActionTemplateId", c => c.Int(nullable: false));
            CreateIndex("dbo.Actions", "ActionTemplateId");
            CreateIndex("dbo.ExternalEventSubscriptions", "ExternalEvent");
            AddForeignKey("dbo.Actions", "ActionTemplateId", "dbo.ActionTemplate", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ExternalEventSubscriptions", "ExternalEvent", "dbo._EventStatusTemplate", "Id", cascadeDelete: true);
        }
    }
}
