namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ActionTemplate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Actions", "Name", c => c.String());
            AddColumn("dbo.Actions", "ActionTemplateId", c => c.Int(nullable: false));
            CreateIndex("dbo.Actions", "ActionTemplateId");
            AddForeignKey("dbo.Actions", "ActionTemplateId", "dbo.ActionTemplate", "Id", cascadeDelete: true);
            DropColumn("dbo.Actions", "ActionType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Actions", "ActionType", c => c.String());
            DropForeignKey("dbo.Actions", "ActionTemplateId", "dbo.ActionTemplate");
            DropIndex("dbo.Actions", new[] { "ActionTemplateId" });
            DropColumn("dbo.Actions", "ActionTemplateId");
            DropColumn("dbo.Actions", "Name");
        }
    }
}
