namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ActionTemplateIdNullable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Actions", "ActionTemplateId", "dbo.ActionTemplate");
            DropIndex("dbo.Actions", new[] { "ActionTemplateId" });
            AlterColumn("dbo.Actions", "ActionTemplateId", c => c.Int());
            CreateIndex("dbo.Actions", "ActionTemplateId");
            AddForeignKey("dbo.Actions", "ActionTemplateId", "dbo.ActionTemplate", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Actions", "ActionTemplateId", "dbo.ActionTemplate");
            DropIndex("dbo.Actions", new[] { "ActionTemplateId" });
            AlterColumn("dbo.Actions", "ActionTemplateId", c => c.Int(nullable: false));
            CreateIndex("dbo.Actions", "ActionTemplateId");
            AddForeignKey("dbo.Actions", "ActionTemplateId", "dbo.ActionTemplate", "Id", cascadeDelete: true);
        }
    }
}
